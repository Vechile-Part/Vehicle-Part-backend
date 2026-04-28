using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Services;

public class AdminService(IAdminRepository repository, INotificationService notificationService) : IAdminService
{
    public async Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default)
    {
        var normalized = reportType?.Trim().ToLowerInvariant();
        if (normalized is not ("daily" or "monthly" or "yearly"))
        {
            throw new ArgumentException("Report type must be daily, monthly, or yearly.", nameof(reportType));
        }

        var now = DateTime.UtcNow;
        var start = normalized switch
        {
            "daily" => now.Date,
            "monthly" => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            "yearly" => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => now.Date
        };

        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);
        var purchases = await repository.GetPurchaseInvoicesAsync(cancellationToken);
        var filteredSales = sales.Where(x => x.IssuedAtUtc >= start).ToList();
        var filteredPurchases = purchases.Where(x => x.IssuedAtUtc >= start).ToList();

        return new FinancialReportDto(
            normalized,
            filteredSales.Sum(x => x.TotalAmount),
            filteredPurchases.Sum(x => x.TotalAmount),
            filteredSales.Sum(x => x.PendingCredit));
    }

    public async Task<IReadOnlyList<object>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        var parts = await repository.GetLowStockPartsAsync(threshold, cancellationToken);
        return parts.Select(p => (object)new
        {
            p.Id,
            p.Name,
            p.PartNumber,
            p.QuantityInStock,
            p.UnitPrice
        }).ToList();
    }
}
