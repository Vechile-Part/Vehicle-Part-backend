using VehiclePart.Application.DTOs.PurchaseInvoice;

namespace VehiclePart.Application.Interfaces;

public interface IPurchaseInvoiceService
{
    Task<PurchaseInvoiceResponseDto> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseInvoiceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PurchaseInvoiceResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}