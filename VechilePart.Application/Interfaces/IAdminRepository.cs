using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface IAdminRepository
{
    Task<IReadOnlyList<Part>> GetPartsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default);
}
