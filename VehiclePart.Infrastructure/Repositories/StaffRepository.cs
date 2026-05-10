using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class StaffRepository(AppDbContext dbContext) : IStaffRepository
{
    public async Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        dbContext.Vehicles.Add(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);
        return vehicle;
    }

    public async Task<SalesInvoice> AddSalesInvoiceAsync(SalesInvoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.SalesInvoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
    }

    public async Task<SalesInvoice?> GetSalesInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesInvoices
            .FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerTerm = searchTerm.ToLower();
        return await dbContext.Customers
            .Where(c => c.FullName.ToLower().Contains(lowerTerm) 
                     || c.Phone.Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default) 
        => await dbContext.Customers.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.Vehicles.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.SalesInvoices.ToListAsync(cancellationToken);
}
