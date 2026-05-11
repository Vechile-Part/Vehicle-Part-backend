using Microsoft.EntityFrameworkCore;
using System.Data;
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

    public async Task<SalesInvoiceItem> AddSalesInvoiceItemAsync(SalesInvoiceItem item, CancellationToken cancellationToken = default)
    {
        dbContext.SalesInvoiceItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<SalesInvoice> CreateSalesInvoiceAtomicAsync(
        SalesInvoice invoice,
        IReadOnlyList<(SalesInvoiceItem Item, Part Part)> lineItems,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            dbContext.SalesInvoices.Add(invoice);
            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var (item, part) in lineItems)
            {
                item.SalesInvoiceId = invoice.Id;
                dbContext.SalesInvoiceItems.Add(item);

                part.QuantityInStock -= item.Quantity;
                dbContext.Parts.Update(part);

                await dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return invoice;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<SalesInvoice?> GetSalesInvoiceByIdAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        => await dbContext.SalesInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);

    public async Task<IReadOnlyList<SalesInvoiceItem>> GetSalesInvoiceItemsAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        => await dbContext.SalesInvoiceItems.Where(x => x.SalesInvoiceId == invoiceId).ToListAsync(cancellationToken);

    public async Task<Part?> GetPartByIdAsync(Guid partId, CancellationToken cancellationToken = default)
        => await dbContext.Parts.FindAsync([partId], cancellationToken);

    public async Task UpdatePartAsync(Part part, CancellationToken cancellationToken = default)
    {
        dbContext.Parts.Update(part);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
        => await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);

    public async Task<IReadOnlyList<Customer>> SearchCustomersAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var lowerTerm = searchTerm.ToLower();
        return await dbContext.Customers
            .Where(c => c.FullName.ToLower().Contains(lowerTerm) || c.Phone.Contains(searchTerm))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default)
        => await dbContext.Customers.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default)
        => await dbContext.Vehicles.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SalesInvoices.ToListAsync(cancellationToken);
}
