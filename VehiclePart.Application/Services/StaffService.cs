using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Security;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Services;

public class StaffService(
    IStaffRepository repository,
    INotificationService notificationService
) : IStaffService
{
    public async Task<Guid> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password is required so the customer can sign in.", nameof(dto));

        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email.Trim(),
            PasswordHash = CustomerPasswordHasher.HashPassword(dto.Password)
        }, cancellationToken);

        await repository.AddVehicleAsync(new Vehicle
        {
            CustomerId = customer.Id,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, cancellationToken);

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

        // Validate and collect parts
        var resolvedItems = new List<(Part Part, int Quantity)>();
        foreach (var line in dto.Items)
        {
            if (line.Quantity <= 0)
                throw new ArgumentException($"Quantity for part '{line.PartId}' must be positive.");

            var part = await repository.GetPartByIdAsync(line.PartId, cancellationToken)
                ?? throw new KeyNotFoundException($"Part '{line.PartId}' not found.");

            if (part.QuantityInStock < line.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for part '{part.Name}'. " +
                    $"Available: {part.QuantityInStock}, requested: {line.Quantity}.");

            resolvedItems.Add((part, line.Quantity));
        }

        decimal subtotal = resolvedItems.Sum(x => x.Part.UnitPrice * x.Quantity);
        if (dto.DiscountAmount > subtotal)
            throw new ArgumentException("Discount cannot exceed the subtotal.", nameof(dto));

        decimal totalAmount = subtotal - dto.DiscountAmount;
        decimal pendingCredit = Math.Max(0, totalAmount - dto.PaidAmount);

        var invoice = await repository.AddSalesInvoiceAsync(new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            TotalAmount = totalAmount,
            DiscountAmount = dto.DiscountAmount,
            PaidAmount = dto.PaidAmount,
            PendingCredit = pendingCredit,
            IssuedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        var itemResponses = new List<SalesInvoiceItemResponseDto>();
        foreach (var (part, qty) in resolvedItems)
        {
            var item = await repository.AddSalesInvoiceItemAsync(new SalesInvoiceItem
            {
                SalesInvoiceId = invoice.Id,
                PartId = part.Id,
                Quantity = qty,
                UnitPrice = part.UnitPrice
            }, cancellationToken);

            part.QuantityInStock -= qty;
            await repository.UpdatePartAsync(part, cancellationToken);

            itemResponses.Add(new SalesInvoiceItemResponseDto(
                item.Id, part.Id, part.Name, qty, part.UnitPrice, qty * part.UnitPrice));
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
        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);
        var regularCustomers = sales.GroupBy(x => x.CustomerId).Count(group => group.Count() >= 3);
        var highSpenders = sales.Where(x => x.TotalAmount > 5000m).Select(x => x.CustomerId).Distinct().Count();
        var pending = sales.Where(x => x.PendingCredit > 0).Select(x => x.CustomerId).Distinct().Count();
        return new CustomerReportDto(regularCustomers, highSpenders, pending);
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
