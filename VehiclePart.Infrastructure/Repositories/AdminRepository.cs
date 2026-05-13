using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class AdminRepository(AppDbContext dbContext) : IAdminRepository
{
    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.SalesInvoices.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.PurchaseInvoices.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Part>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default)
        => await dbContext.Parts.Where(x => x.QuantityInStock < threshold).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<OverdueCreditInvoiceDto>> GetOverdueCreditInvoicesAsync(
        int minimumAgeMonths,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddMonths(-minimumAgeMonths);

        return await (
            from invoice in dbContext.SalesInvoices.AsNoTracking()
            join customer in dbContext.Customers.AsNoTracking() on invoice.CustomerId equals customer.Id
            where invoice.PendingCredit > 0
                  && invoice.IssuedAtUtc <= cutoff
            orderby invoice.IssuedAtUtc
            select new OverdueCreditInvoiceDto(
                invoice.Id,
                customer.Id,
                customer.FullName,
                customer.Email,
                invoice.PendingCredit,
                invoice.IssuedAtUtc,
                invoice.LastCreditReminderSentUtc)
        ).ToListAsync(cancellationToken);
    }

    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Users.FindAsync(id, cancellationToken);

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        => await dbContext.Users.ToListAsync(cancellationToken);

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync(id, cancellationToken);
        if (user is not null)
        {
            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddPartAsync(Part part, CancellationToken cancellationToken = default)
    {
        dbContext.Parts.Add(part);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Part?> GetPartByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Parts.FindAsync(id, cancellationToken);

    public async Task UpdatePartAsync(Part part, CancellationToken cancellationToken = default)
    {
        dbContext.Parts.Update(part);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await dbContext.Parts.FindAsync(id, cancellationToken);
        if (part is not null)
        {
            dbContext.Parts.Remove(part);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task AddPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.PurchaseInvoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
