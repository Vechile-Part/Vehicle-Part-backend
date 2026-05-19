using VehiclePart.Application.Common;
using VehiclePart.Application.Formatting;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Security;
using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;

namespace VehiclePart.Application.Services;

public class StaffService(
    IStaffRepository repository,
    ICustomerRepository customerRepository,
    ICustomerHistoryRepository customerHistoryRepository,
    INotificationService notificationService,
    ICustomerInviteService customerInviteService,
    IAdminRepository adminRepository
) : IStaffService
{
    public async Task<Guid> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required.", nameof(dto));

        if (await customerRepository.GetCustomerByEmailAsync(dto.Email, cancellationToken) is not null)
            throw new InvalidOperationException("A customer with this email already exists.");

        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email.Trim(),
            PasswordHash = string.Empty
        }, cancellationToken);

        await repository.AddVehicleAsync(new Vehicle
        {
            CustomerId = customer.Id,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, cancellationToken);

        await customerInviteService.SendPasswordSetupInviteAsync(customer.Id, customer.Email, cancellationToken);

        return customer.Id;
    }

    
    public async Task<SalesInvoiceResponseDto> CreateSalesInvoiceAsync(
        SalesInvoiceCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.Items is null || dto.Items.Count == 0)
            throw new ArgumentException("A sales invoice must have at least one item.", nameof(dto));

        if (dto.DiscountAmount < 0)
            throw new ArgumentException("Discount cannot be negative.", nameof(dto));

        foreach (var line in dto.Items)
        {
            if (line.Quantity <= 0)
                throw new ArgumentException($"Quantity for part '{line.PartId}' must be positive.");
        }

        var result = await repository.ExecuteInTransactionAsync(
            async ct => await CreateSalesInvoiceCoreAsync(dto, ct),
            cancellationToken);

        var emailSent = false;
        string? emailError = null;
        try
        {
            await SendInvoiceEmailAsync(result.Id, cancellationToken);
            emailSent = true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            emailError = ex.Message;
        }

        return result with { EmailSent = emailSent, EmailError = emailError };
    }

    private async Task<SalesInvoiceResponseDto> CreateSalesInvoiceCoreAsync(
        SalesInvoiceCreateDto dto,
        CancellationToken cancellationToken)
    {
        var quantityByPartId = dto.Items
            .GroupBy(x => x.PartId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var parts = new Dictionary<Guid, Part>();
        foreach (var (partId, totalQty) in quantityByPartId)
        {
            var part = await repository.GetPartByIdAsync(partId, cancellationToken)
                ?? throw new KeyNotFoundException($"Part '{partId}' not found.");

            if (part.QuantityInStock < totalQty)
                throw new InvalidOperationException(
                    $"Insufficient stock for part '{part.Name}'. " +
                    $"Available: {part.QuantityInStock}, requested: {totalQty}.");

            parts[partId] = part;
        }

        decimal subtotal = dto.Items.Sum(line =>
        {
            var part = parts[line.PartId];
            return part.UnitPrice * line.Quantity;
        });

        if (dto.DiscountAmount > subtotal)
            throw new ArgumentException("Discount cannot exceed the subtotal.", nameof(dto));

        if (dto.PaidAmount < 0)
            throw new ArgumentException("Paid amount cannot be negative.", nameof(dto));
        const decimal loyaltySpendThreshold = 5000m;
        const decimal loyaltyRate = 0.10m;
        var loyaltyMinimumDiscount = subtotal > loyaltySpendThreshold
            ? Math.Round(subtotal * loyaltyRate, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var appliedDiscount = Math.Max(dto.DiscountAmount, loyaltyMinimumDiscount);
        if (appliedDiscount > subtotal)
            appliedDiscount = subtotal;

        decimal totalAmount = subtotal - appliedDiscount;
        if (dto.PaidAmount > totalAmount)
            throw new ArgumentException("Paid amount cannot exceed the invoice total.", nameof(dto));
        decimal pendingCredit = Math.Max(0, totalAmount - dto.PaidAmount);

        var invoiceNumber = await repository.ReserveNextInvoiceNumberAsync(cancellationToken);

        var invoice = await repository.AddSalesInvoiceAsync(new SalesInvoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = dto.CustomerId,
            TotalAmount = totalAmount,
            DiscountAmount = appliedDiscount,
            PaidAmount = dto.PaidAmount,
            PendingCredit = pendingCredit,
            IssuedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        var itemResponses = new List<SalesInvoiceItemResponseDto>();
        foreach (var line in dto.Items)
        {
            var part = parts[line.PartId];
            var item = await repository.AddSalesInvoiceItemAsync(new SalesInvoiceItem
            {
                SalesInvoiceId = invoice.Id,
                PartId = part.Id,
                Quantity = line.Quantity,
                UnitPrice = part.UnitPrice
            }, cancellationToken);

            itemResponses.Add(new SalesInvoiceItemResponseDto(
                item.Id, part.Id, part.Name, line.Quantity, part.UnitPrice, line.Quantity * part.UnitPrice));
        }

        foreach (var (partId, totalQty) in quantityByPartId)
        {
            var affected = await repository.TryDecrementPartStockAsync(partId, totalQty, cancellationToken);
            if (affected != 1)
            {
                var part = parts[partId];
                throw new InvalidOperationException(
                    $"Could not reserve stock for part '{part.Name}'. " +
                    "Another sale may have completed first, or available quantity changed.");
            }
        }

        return new SalesInvoiceResponseDto(
            invoice.Id, invoice.InvoiceNumber, invoice.CustomerId, invoice.IssuedAtUtc,
            invoice.TotalAmount, invoice.DiscountAmount, invoice.PaidAmount,
            invoice.PendingCredit, itemResponses);
    }

    public async Task<SalesInvoiceResponseDto?> GetSalesInvoiceAsync(
        Guid invoiceId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await repository.GetSalesInvoiceByIdAsync(invoiceId, cancellationToken);
        if (invoice is null) return null;

        var items = await repository.GetSalesInvoiceItemsAsync(invoiceId, cancellationToken);

        var itemDtos = new List<SalesInvoiceItemResponseDto>();
        foreach (var item in items)
        {
            var part = await repository.GetPartByIdAsync(item.PartId, cancellationToken);
            itemDtos.Add(new SalesInvoiceItemResponseDto(
                item.Id, item.PartId, part?.Name ?? "Unknown",
                item.Quantity, item.UnitPrice, item.Quantity * item.UnitPrice));
        }

        return new SalesInvoiceResponseDto(
            invoice.Id, invoice.InvoiceNumber, invoice.CustomerId, invoice.IssuedAtUtc,
            invoice.TotalAmount, invoice.DiscountAmount, invoice.PaidAmount,
            invoice.PendingCredit, itemDtos);
    }

    public async Task<IReadOnlyList<SalesInvoiceSummaryDto>> ListSalesInvoicesAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await repository.ListSalesInvoicesWithCustomerAsync(cancellationToken);
        return rows
            .Select(row => new SalesInvoiceSummaryDto(
                row.Invoice.Id,
                string.IsNullOrWhiteSpace(row.Invoice.InvoiceNumber)
                    ? $"INV-{row.Invoice.IssuedAtUtc:yyyy}-{row.Invoice.Id.ToString()[..3].ToUpperInvariant()}"
                    : row.Invoice.InvoiceNumber,
                row.Invoice.CustomerId,
                row.CustomerName,
                row.CustomerPhone,
                row.Invoice.IssuedAtUtc,
                row.Invoice.TotalAmount,
                row.Invoice.DiscountAmount,
                row.Invoice.PaidAmount,
                row.Invoice.PendingCredit))
            .ToList();
    }

    public async Task<object?> GetCustomerDetailsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var history = await customerHistoryRepository.GetCustomerHistoryAsync(
            customerId,
            includeAppointmentsAndReviews: false,
            cancellationToken);
        if (history is null) return null;

        return new
        {
            Id = history.CustomerId,
            FullName = history.CustomerName,
            history.Phone,
            history.Email,
            vehicles = history.Vehicles.Select(v => new
            {
                v.VehicleId,
                Id = v.VehicleId,
                v.VehicleNumber,
                v.Make,
                v.Model,
                v.Year,
            }),
            invoices = history.Invoices.Select(i => new
            {
                Id = i.InvoiceId,
                i.InvoiceId,
                i.IssuedAtUtc,
                i.TotalAmount,
                i.DiscountAmount,
                i.PaidAmount,
                i.PendingCredit,
                purchasedItems = i.Items.Select(x => x.PartName).ToList(),
                items = i.Items.Select(x => new
                {
                    partName = x.PartName,
                    name = x.PartName,
                    x.Quantity,
                    x.UnitPrice,
                }),
            }),
        };
    }

    public async Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default)
    {
        var sales = (await repository.GetSalesInvoicesAsync(cancellationToken)).ToList();
        var customers = (await repository.GetCustomersAsync(cancellationToken)).ToDictionary(c => c.Id);

        string FullName(Guid id) => customers.TryGetValue(id, out var c) ? c.FullName : string.Empty;
        string PhoneOf(Guid id) => customers.TryGetValue(id, out var c) ? c.Phone : string.Empty;
        string EmailOf(Guid id) => customers.TryGetValue(id, out var c) ? c.Email : string.Empty;

        CustomerReportRowDto Row(Guid customerId, List<SalesInvoice> list)
        {
            return new CustomerReportRowDto(
                customerId,
                FullName(customerId),
                PhoneOf(customerId),
                EmailOf(customerId),
                list.Count,
                list.Sum(x => x.TotalAmount),
                list.Count == 0 ? 0m : list.Max(x => x.TotalAmount),
                list.Sum(x => x.PendingCredit));
        }

        var byCustomer = sales.GroupBy(x => x.CustomerId).ToDictionary(g => g.Key, g => g.ToList());

        var regularRows = byCustomer
            .Where(g => g.Value.Count >= 3)
            .Select(g => Row(g.Key, g.Value))
            .OrderByDescending(r => r.LifetimeSalesTotal)
            .ToList();

        var highSpenderIds = sales.Where(x => x.TotalAmount > 5000m).Select(x => x.CustomerId).Distinct().ToList();
        var highRows = highSpenderIds
            .Select(id => Row(id, byCustomer.GetValueOrDefault(id) ?? []))
            .OrderByDescending(r => r.LargestInvoiceTotal)
            .ToList();

        var pendingIds = sales.Where(x => x.PendingCredit > 0m).Select(x => x.CustomerId).Distinct().ToList();
        var pendingRows = pendingIds
            .Select(id => Row(id, byCustomer.GetValueOrDefault(id) ?? []))
            .Where(r => r.TotalOutstandingCredit > 0m)
            .OrderByDescending(r => r.TotalOutstandingCredit)
            .ToList();

        return new CustomerReportDto(
            regularRows.Count,
            highRows.Count,
            pendingRows.Count,
            regularRows,
            highRows,
            pendingRows);
    }

   
    public async Task<IReadOnlyList<object>> SearchCustomersAsync(
        CustomerSearchDto dto,
        CancellationToken cancellationToken = default)
    {
        var rows = await repository.SearchCustomersFilteredAsync(
            dto.Phone,
            dto.FullName,
            dto.VehicleNumber,
            dto.CustomerId,
            cancellationToken);

        return rows.Select(row => (object)new
        {
            row.Id,
            row.FullName,
            row.Phone,
            row.Email,
            row.VehicleNumber,
            row.Make,
            row.Model,
            row.Year,
        }).ToList();
    }

    public async Task SendInvoiceEmailAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await repository.GetSalesInvoiceByIdAsync(invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice not found.");

        var customer = await repository.GetCustomerAsync(invoice.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Customer not found.");

        if (string.IsNullOrWhiteSpace(customer.Email))
            throw new InvalidOperationException("Customer email is missing.");

        var items = await repository.GetSalesInvoiceItemsAsync(invoiceId, cancellationToken);
        var emailLines = new List<SalesInvoiceEmailLine>();
        foreach (var item in items)
        {
            var part = await repository.GetPartByIdAsync(item.PartId, cancellationToken);
            var partName = part?.Name ?? "Part";
            var lineTotal = item.Quantity * item.UnitPrice;
            emailLines.Add(new SalesInvoiceEmailLine(partName, item.Quantity, item.UnitPrice, lineTotal));
        }

        var displayNumber = string.IsNullOrWhiteSpace(invoice.InvoiceNumber)
            ? invoice.Id.ToString()[..8].ToUpperInvariant()
            : invoice.InvoiceNumber;
        var subject = $"Sales invoice · {displayNumber}";
        var body = SalesInvoiceEmailTemplate.Build(
            customer.FullName,
            displayNumber,
            invoice.IssuedAtUtc,
            emailLines,
            invoice.TotalAmount + invoice.DiscountAmount,
            invoice.DiscountAmount,
            invoice.TotalAmount,
            invoice.PaidAmount,
            invoice.PendingCredit);

        if (!await notificationService.TrySendEmailAsync(customer.Email, subject, body, cancellationToken))
            throw new InvalidOperationException(
                "Invoice email could not be sent. Check SMTP settings in server configuration.");
    }

    public Task<IReadOnlyList<StaffAppointmentDto>> ListAppointmentsAsync(CancellationToken cancellationToken = default)
        => customerRepository.GetStaffAppointmentsAsync(cancellationToken);

    public async Task UpdateAppointmentStatusAsync(
        Guid appointmentId,
        UpdateAppointmentStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        if (appointmentId == Guid.Empty)
            throw new ArgumentException("Invalid appointment ID.");

        var status = AppointmentStatuses.Normalize(dto.Status);
        var appointment = await customerRepository.GetAppointmentByIdForUpdateAsync(appointmentId, cancellationToken)
            ?? throw new InvalidOperationException("Appointment not found.");

        appointment.Status = status;
        await customerRepository.UpdateAppointmentAsync(appointment, cancellationToken);
    }

    public async Task<StaffProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await RequireStaffUserAsync(userId, cancellationToken);
        return new StaffProfileDto(user.Id, user.FullName, user.Email, user.Phone);
    }

    public async Task UpdateMyProfileAsync(
        Guid userId,
        UpdateStaffSelfProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await RequireStaffUserAsync(userId, cancellationToken);

        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new ArgumentException("Full name is required.", nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required.", nameof(dto));

        var email = dto.Email.Trim();
        var users = await adminRepository.GetAllUsersAsync(cancellationToken);
        if (users.Any(u => u.Id != userId && string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Another account already uses this email.");

        user.FullName = dto.FullName.Trim();
        user.Email = email;
        user.Phone = dto.Phone?.Trim() ?? string.Empty;
        await adminRepository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task ChangeMyPasswordAsync(
        Guid userId,
        ChangeStaffPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            throw new ArgumentException("New password must be at least 8 characters.", nameof(dto));

        var user = await RequireStaffUserAsync(userId, cancellationToken);

        if (!CustomerPasswordHasher.VerifyPassword(dto.CurrentPassword, user.Password))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.Password = CustomerPasswordHasher.HashPassword(dto.NewPassword);
        await adminRepository.UpdateUserAsync(user, cancellationToken);
    }

    private async Task<User> RequireStaffUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await adminRepository.GetUserByIdAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Role != RoleType.Staff)
            throw new UnauthorizedAccessException("Only staff accounts can use this profile.");

        return user;
    }
}
