using Microsoft.EntityFrameworkCore;
using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public class AdminRepository(AppDbContext dbContext) : IAdminRepository
{
    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.SalesInvoices.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.PurchaseInvoices.ToListAsync(cancellationToken);
}
