namespace VechilePart.Application.DTOs;

public record PartDto(Guid Id, string Name, string PartNumber, decimal UnitPrice, int QuantityInStock, Guid VendorId);
public record FinancialReportDto(string ReportType, decimal TotalSales, decimal TotalPurchases, decimal PendingCredits);
public record CustomerSummaryDto(Guid Id, string FullName, string Phone, decimal TotalSpent, decimal TotalPending, int InvoiceCount);
public record CustomerReportDto(
    int RegularCustomersCount,
    int HighSpendersCount,
    int PendingCreditsCount,
    List<CustomerSummaryDto> Regulars,
    List<CustomerSummaryDto> HighSpenders,
    List<CustomerSummaryDto> PendingCredits);
