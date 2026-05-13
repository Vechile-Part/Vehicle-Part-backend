using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Services;

public class StaffService(
    IStaffRepository repository,
    INotificationService notificationService,
    ICustomerInviteService customerInviteService
) : IStaffService
{
    public async Task<Guid> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required.", nameof(dto));

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

        return await repository.ExecuteInTransactionAsync(
            async ct => await CreateSalesInvoiceCoreAsync(dto, ct),
            cancellationToken);
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

        const decimal loyaltySpendThreshold = 5000m;
        const decimal loyaltyRate = 0.10m;
        var loyaltyMinimumDiscount = subtotal > loyaltySpendThreshold
            ? Math.Round(subtotal * loyaltyRate, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var appliedDiscount = Math.Max(dto.DiscountAmount, loyaltyMinimumDiscount);
        if (appliedDiscount > subtotal)
            appliedDiscount = subtotal;

        decimal totalAmount = subtotal - appliedDiscount;
        decimal pendingCredit = Math.Max(0, totalAmount - dto.PaidAmount);

        var invoice = await repository.AddSalesInvoiceAsync(new SalesInvoice
        {
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
            invoice.Id, invoice.CustomerId, invoice.IssuedAtUtc,
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
            invoice.Id, invoice.CustomerId, invoice.IssuedAtUtc,
            invoice.TotalAmount, invoice.DiscountAmount, invoice.PaidAmount,
            invoice.PendingCredit, itemDtos);
    }

    public async Task<object?> GetCustomerDetailsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetCustomerAsync(customerId, cancellationToken);
        if (customer is null) return null;

        var vehicles = (await repository.GetVehiclesAsync(cancellationToken))
            .Where(x => x.CustomerId == customerId).ToList();

        var invoices = (await repository.GetSalesInvoicesAsync(cancellationToken))
            .Where(x => x.CustomerId == customerId).ToList();

        return new { customer.Id, customer.FullName, customer.Phone, customer.Email, vehicles, invoices };
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
        
        if (dto.CustomerId.HasValue)
        {
            var customer = await repository.GetCustomerAsync(dto.CustomerId.Value, cancellationToken);
            if (customer is null) return [];
            return [(object)new { customer.Id, customer.FullName, customer.Phone, customer.Email }];
        }

        var customers = await repository.GetCustomersAsync(cancellationToken);
        var vehicles = await repository.GetVehiclesAsync(cancellationToken);

        var query = customers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(dto.Phone))
            query = query.Where(c => c.Phone.Contains(dto.Phone, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            query = query.Where(c => c.FullName.Contains(dto.FullName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(dto.VehicleNumber))
        {
            var customerIds = vehicles
                .Where(v => v.VehicleNumber.Contains(dto.VehicleNumber, StringComparison.OrdinalIgnoreCase))
                .Select(v => v.CustomerId)
                .ToHashSet();

            query = query.Where(c => customerIds.Contains(c.Id));
        }

        return query.Select(c => (object)new { c.Id, c.FullName, c.Phone, c.Email }).ToList();
    }

    public async Task SendInvoiceEmailAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await repository.GetSalesInvoiceByIdAsync(invoiceId, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice not found.");

        var customer = await repository.GetCustomerAsync(invoice.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException("Customer not found.");

        if (string.IsNullOrWhiteSpace(customer.Email))
            throw new InvalidOperationException("Customer email is missing.");

        var subject = $"Invoice #{invoice.Id}";
        var body = $"""
        <h2>Vehicle Parts Invoice</h2>
        <p>Hello {customer.FullName},</p>
        <p>Your invoice details are below:</p>
        <ul>
            <li>Total Amount: {invoice.TotalAmount:C}</li>
            <li>Discount: {invoice.DiscountAmount:C}</li>
            <li>Paid Amount: {invoice.PaidAmount:C}</li>
            <li>Pending Credit: {invoice.PendingCredit:C}</li>
            <li>Issued Date: {invoice.IssuedAtUtc:yyyy-MM-dd HH:mm} UTC</li>
        </ul>
        <p>Thank you.</p>
        """;

        await notificationService.SendEmailAsync(customer.Email, subject, body);
    }
}
