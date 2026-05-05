using System.Security.Cryptography;
using System.Text;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Services;

public class StaffService(IStaffRepository repository) : IStaffService
{
    public async Task<Guid> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password)
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
        if (!string.IsNullOrWhiteSpace(dto.FullName)) query = query.Where(c => c.FullName.Contains(dto.FullName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(dto.VehicleNumber))
        {
            var customerIds = vehicles.Where(v => v.VehicleNumber.Contains(dto.VehicleNumber, StringComparison.OrdinalIgnoreCase)).Select(v => v.CustomerId).ToHashSet();
            query = query.Where(c => customerIds.Contains(c.Id));
        }
        return query.Select(c => (object)c).ToList();
    }

    public Task SendInvoiceEmailAsync(Guid invoiceId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    private static string HashPassword(string password)
    {
        const int iterations = 100_000;
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            32);
        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}
