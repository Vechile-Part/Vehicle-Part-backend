using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace VechilePart.Application.Services;

public class StaffService(IStaffRepository repository) : IStaffService
{
    public async Task<CustomerReportDto> GetCustomerReportAsync(CancellationToken cancellationToken = default)
    {
        var customers = await repository.GetCustomersAsync(cancellationToken);
        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);

        var summaries = customers.Select(c => {
            var customerSales = sales.Where(s => s.CustomerId == c.Id).ToList();
            return new CustomerSummaryDto(
                c.Id,
                c.FullName,
                c.Phone,
                customerSales.Sum(s => s.TotalAmount),
                customerSales.Sum(s => s.PendingCredit),
                customerSales.Count
            );
        }).ToList();

        var regulars = summaries.Where(s => s.InvoiceCount >= 3).ToList();
        var highSpenders = summaries.Where(s => s.TotalSpent > 5000m).ToList();
        var pendingCredits = summaries.Where(s => s.TotalPending > 0).ToList();

        return new CustomerReportDto(
            regulars.Count,
            highSpenders.Count,
            pendingCredits.Count,
            regulars,
            highSpenders,
            pendingCredits
        );
    }
}
