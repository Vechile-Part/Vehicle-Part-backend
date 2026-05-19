using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class PurchaseInvoiceRepository(AppDbContext dbContext) : IPurchaseInvoiceRepository
{
    public async Task<PurchaseInvoice> CreateAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.ChangeTracker.Clear();
        dbContext.PurchaseInvoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task<IReadOnlyList<PurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.PurchaseInvoices
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.Items)
            .ThenInclude(i => i.Part)
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<PurchaseInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.PurchaseInvoices
            .Include(x => x.Vendor)
            .Include(x => x.Items)
            .ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}