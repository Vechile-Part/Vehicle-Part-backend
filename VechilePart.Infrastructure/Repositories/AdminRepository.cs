using Microsoft.EntityFrameworkCore;
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
}
