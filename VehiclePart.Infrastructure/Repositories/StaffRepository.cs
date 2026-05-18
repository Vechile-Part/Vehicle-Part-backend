using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class StaffRepository(AppDbContext dbContext) : IStaffRepository
{
    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.CurrentTransaction is not null)
            return await operation(cancellationToken);

        await using IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

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

    public async Task<string> ReserveNextInvoiceNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";
        var existingNumbers = await dbContext.SalesInvoices.AsNoTracking()
            .Where(invoice => invoice.InvoiceNumber.StartsWith(prefix))
            .Select(invoice => invoice.InvoiceNumber)
            .ToListAsync(cancellationToken);

        var nextSequence = 1;
        foreach (var number in existingNumbers)
        {
            var suffix = number.Length > prefix.Length ? number[prefix.Length..] : string.Empty;
            if (int.TryParse(suffix, out var parsed) && parsed >= nextSequence)
                nextSequence = parsed + 1;
        }

        return $"{prefix}{nextSequence:D3}";
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

    public Task<int> TryDecrementPartStockAsync(Guid partId, int quantity, CancellationToken cancellationToken = default) =>
        dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE "Parts"
            SET "QuantityInStock" = "QuantityInStock" - {quantity}
            WHERE "Id" = {partId} AND "QuantityInStock" >= {quantity}
            """,
            cancellationToken);

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

    public async Task<IReadOnlyList<(SalesInvoice Invoice, string CustomerName, string CustomerPhone)>> ListSalesInvoicesWithCustomerAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await (
            from invoice in dbContext.SalesInvoices.AsNoTracking()
            join customer in dbContext.Customers.AsNoTracking() on invoice.CustomerId equals customer.Id into customerJoin
            from customer in customerJoin.DefaultIfEmpty()
            orderby invoice.IssuedAtUtc descending
            select new
            {
                Invoice = invoice,
                CustomerName = customer != null ? customer.FullName : "Unknown customer",
                CustomerPhone = customer != null ? customer.Phone : string.Empty,
            }).ToListAsync(cancellationToken);

        return rows.Select(row => (row.Invoice, row.CustomerName, row.CustomerPhone)).ToList();
    }
}
