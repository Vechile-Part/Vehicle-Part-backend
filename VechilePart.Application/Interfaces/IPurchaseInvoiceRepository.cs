using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;

public interface IPurchaseInvoiceRepository
{
    Task<bool> VendorExistsAsync(Guid vendorId, CancellationToken cancellationToken = default);

    Task<PurchaseInvoice> CreateAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PurchaseInvoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PurchaseInvoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}