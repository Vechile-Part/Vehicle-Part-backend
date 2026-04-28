using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface IAdminService
{
    Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default);
}
