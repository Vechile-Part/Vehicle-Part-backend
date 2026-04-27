namespace VechilePart.Application.DTOs;

public record FinancialReportDto(string ReportType, decimal TotalSales, decimal TotalPurchases, decimal PendingCredits);
