using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Services;

public class StaffService(IStaffRepository repository) : IStaffService
{
    public async Task RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            GovernmentId = dto.GovernmentId
        }, cancellationToken);

        _ = await repository.AddVehicleAsync(new Vehicle
        {
            CustomerId = customer.Id,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, cancellationToken);
    }

    public async Task<Guid> CreateSalesInvoiceAsync(SalesInvoiceCreateDto dto, CancellationToken cancellationToken = default)
    {
        var discount = dto.TotalAmount > 5000m ? dto.TotalAmount * 0.10m : 0m;
        var finalAmount = dto.TotalAmount - discount;
        var pending = Math.Max(0, finalAmount - dto.PaidAmount);

        var invoice = await repository.AddSalesInvoiceAsync(new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            TotalAmount = finalAmount,
            DiscountAmount = discount,
            PaidAmount = dto.PaidAmount,
            PendingCredit = pending
        }, cancellationToken);

        return invoice.Id;
    }

    public async Task<object?> GetCustomerDetailsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetCustomerAsync(customerId, cancellationToken);
        if (customer is null) return null;

        var vehicles = (await repository.GetVehiclesAsync(cancellationToken)).Where(x => x.CustomerId == customerId).ToList();
        var invoices = (await repository.GetSalesInvoicesAsync(cancellationToken)).Where(x => x.CustomerId == customerId).ToList();

        return new { customer, vehicles, invoices };
    }

    public async Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default)
    {
        var customers = await repository.GetCustomersAsync(cancellationToken);
        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);

        var highSpenders = sales.Where(x => x.TotalAmount > 5000m).Select(x => x.CustomerId).Distinct().Count();
        var pending = sales.Where(x => x.PendingCredit > 0).Select(x => x.CustomerId).Distinct().Count();

        return new CustomerReportDto(customers.Count, highSpenders, pending);
    }

    public async Task<IReadOnlyList<object>> SearchCustomersAsync(CustomerSearchDto dto, CancellationToken cancellationToken = default)
    {
        var customers = await repository.GetCustomersAsync(cancellationToken);
        var vehicles = await repository.GetVehiclesAsync(cancellationToken);

        var query = customers.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(dto.Phone)) query = query.Where(c => c.Phone.Contains(dto.Phone, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(dto.GovernmentId)) query = query.Where(c => c.GovernmentId.Contains(dto.GovernmentId, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(dto.FullName)) query = query.Where(c => c.FullName.Contains(dto.FullName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(dto.VehicleNumber))
        {
            var customerIds = vehicles.Where(v => v.VehicleNumber.Contains(dto.VehicleNumber, StringComparison.OrdinalIgnoreCase)).Select(v => v.CustomerId).ToHashSet();
            query = query.Where(c => customerIds.Contains(c.Id));
        }

        return query.Select(c => (object)c).ToList();
    }

    public Task SendInvoiceEmailAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
