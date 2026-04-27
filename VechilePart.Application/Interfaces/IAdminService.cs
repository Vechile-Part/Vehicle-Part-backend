using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface IAdminService
{
    Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default);
    Task RegisterStaffAsync(StaffRegistrationDto dto, CancellationToken cancellationToken = default);
    Task<PartDto> UpsertPartAsync(PartDto dto, CancellationToken cancellationToken = default);
    Task DeletePartAsync(Guid partId, CancellationToken cancellationToken = default);
    Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(PurchaseInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<VendorDto> UpsertVendorAsync(VendorDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartDto>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default);
}
