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

    public async Task<Guid> CreateSalesInvoiceAsync(SalesInvoiceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var pending = Math.Max(0, dto.TotalAmount - dto.PaidAmount);

        var invoice = await repository.AddSalesInvoiceAsync(new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            TotalAmount = dto.TotalAmount,
            DiscountAmount = 0m,
            PaidAmount = dto.PaidAmount,
            PendingCredit = pending
        }, cancellationToken);

        return invoice.Id;
    }

    public async Task<object?> GetCustomerDetailsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetCustomerAsync(customerId, cancellationToken);
        if (customer is null) return null;

        var vehicles = (await repository.GetVehiclesAsync(cancellationToken))
            .Where(x => x.CustomerId == customerId)
            .ToList();

        var invoices = (await repository.GetSalesInvoicesAsync(cancellationToken))
            .Where(x => x.CustomerId == customerId)
            .ToList();

        return new
        {
            customer.Id,
            customer.FullName,
            customer.Phone,
            customer.Email,
            vehicles,
            invoices
        };
    }

    public async Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default)
    {
        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);

        var regularCustomers = sales.GroupBy(x => x.CustomerId).Count(group => group.Count() >= 3);
        var highSpenders = sales.Where(x => x.TotalAmount > 5000m).Select(x => x.CustomerId).Distinct().Count();
        var pending = sales.Where(x => x.PendingCredit > 0).Select(x => x.CustomerId).Distinct().Count();

        return new CustomerReportDto(regularCustomers, highSpenders, pending);
    }

    public async Task<IReadOnlyList<object>> SearchCustomersAsync(CustomerSearchDto dto, CancellationToken cancellationToken = default)
    {
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

        return query.Select(c => (object)new
        {
            c.Id,
            c.FullName,
            c.Phone,
            c.Email
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

        var subject = $"Invoice #{invoice.Id}";

        var body = $"""
        <h2>Vehicle Parts Invoice</h2>
        <p>Hello {customer.FullName},</p>
        <p>Your invoice details are below:</p>
        <ul>
            <li>Total Amount: {invoice.TotalAmount}</li>
            <li>Paid Amount: {invoice.PaidAmount}</li>
            <li>Pending Credit: {invoice.PendingCredit}</li>
            <li>Issued Date: {invoice.IssuedAtUtc}</li>
        </ul>
        <p>Thank you.</p>
        """;

        await notificationService.SendEmailAsync(customer.Email, subject, body);
    }
}
