using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace VechilePart.Application.Services;

public class AdminService(IAdminRepository repository) : IAdminService
{
    public async Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default)
    {
        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);
        var purchases = await repository.GetPurchaseInvoicesAsync(cancellationToken);

        return new FinancialReportDto(
            reportType,
            sales.Sum(x => x.TotalAmount),
            purchases.Sum(x => x.TotalAmount),
            sales.Sum(x => x.PendingCredit));
    }
}
