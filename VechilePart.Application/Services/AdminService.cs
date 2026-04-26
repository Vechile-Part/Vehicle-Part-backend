using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace VechilePart.Application.Services;

public class AdminService(IAdminRepository repository) : IAdminService
{
    public async Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default)
    {
        var allSales = await repository.GetSalesInvoicesAsync(cancellationToken);
        var allPurchases = await repository.GetPurchaseInvoicesAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var sales = allSales.Where(x =>
            reportType.Equals("Daily", StringComparison.OrdinalIgnoreCase) ? x.IssuedAtUtc.Date == now.Date :
            reportType.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ? x.IssuedAtUtc.Year == now.Year && x.IssuedAtUtc.Month == now.Month :
            reportType.Equals("Yearly", StringComparison.OrdinalIgnoreCase) ? x.IssuedAtUtc.Year == now.Year : true).ToList();

        var purchases = allPurchases.Where(x =>
            reportType.Equals("Daily", StringComparison.OrdinalIgnoreCase) ? x.IssuedAtUtc.Date == now.Date :
            reportType.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ? x.IssuedAtUtc.Year == now.Year && x.IssuedAtUtc.Month == now.Month :
            reportType.Equals("Yearly", StringComparison.OrdinalIgnoreCase) ? x.IssuedAtUtc.Year == now.Year : true).ToList();

        return new FinancialReportDto(
            reportType,
            sales.Sum(x => x.TotalAmount),
            purchases.Sum(x => x.TotalAmount),
            sales.Sum(x => x.PendingCredit));
    }

    public async Task<IReadOnlyList<PartDto>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        var parts = await repository.GetPartsAsync(cancellationToken);
        return parts
            .Where(p => p.QuantityInStock < threshold)
            .Select(p => new PartDto(p.Id, p.Name, p.PartNumber, p.UnitPrice, p.QuantityInStock, p.VendorId))
            .ToList();
    }
}
