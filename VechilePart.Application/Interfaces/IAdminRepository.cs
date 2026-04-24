using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface IAdminRepository
{
    Task<User> AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<Part> UpsertPartAsync(Part part, CancellationToken cancellationToken = default);
    Task DeletePartAsync(Guid partId, CancellationToken cancellationToken = default);
    Task<PurchaseInvoice> AddPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default);
    Task<Vendor> UpsertVendorAsync(Vendor vendor, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Part>> GetPartsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default);
}
