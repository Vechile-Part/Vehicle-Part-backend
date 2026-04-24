using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public class StaffRepository(AppDbContext dbContext) : IStaffRepository
{
    public Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(customer);
        return Task.FromResult(customer);
    }

    public Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        dbContext.Vehicles.Add(vehicle);
        return Task.FromResult(vehicle);
    }

    public Task<SalesInvoice> AddSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.SalesInvoices.Add(invoice);
        return Task.FromResult(invoice);
    }

    public Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(dbContext.Customers.FirstOrDefault(x => x.Id == customerId));
    }

    public Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<Customer>)dbContext.Customers);
    public Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<Vehicle>)dbContext.Vehicles);
    public Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<SalesInvoice>)dbContext.SalesInvoices);
}
