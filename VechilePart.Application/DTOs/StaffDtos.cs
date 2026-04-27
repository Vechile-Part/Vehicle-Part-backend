namespace VechilePart.Application.DTOs;

public record CustomerSummaryDto(
    Guid CustomerId,
    string FullName,
    string Phone,
    decimal TotalSpent,
    decimal TotalPending,
    int InvoiceCount);

public record CustomerReportDto(
    int RegularCustomers,
    int HighSpenders,
    int CustomersWithPendingCredits,
    IReadOnlyList<CustomerSummaryDto> RegularCustomerList,
    IReadOnlyList<CustomerSummaryDto> HighSpenderList,
    IReadOnlyList<CustomerSummaryDto> PendingCreditList);
