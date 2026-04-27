using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface IAdminService
{
    Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default);
}
