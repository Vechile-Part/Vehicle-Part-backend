namespace VehiclePart.Application.DTOs;
using VehiclePart.Domain.Enums;

public record FinancialReportDto(string ReportType, decimal TotalSales, decimal TotalPurchases, decimal PendingCredits);

/// <summary>One row or chart bar: revenue (sales), operating costs (purchases), net profit, and margin status.</summary>
public record FinancialBucketDto(string Label, DateTime DateUtc, decimal GrossRevenue, decimal OperatingCosts, decimal NetProfit, string Status);

/// <summary>Admin financial dashboard: buckets for chart/table plus summary KPIs.</summary>
public record FinancialDashboardDto(
    string Period,
    IReadOnlyList<FinancialBucketDto> ChartBuckets,
    IReadOnlyList<FinancialBucketDto> TableRows,
    decimal TotalNetProfit,
    decimal PreviousPeriodNetProfit,
    decimal EstimatedTax,
    int PendingInvoiceCount,
    decimal TotalPendingCredits);
public record StaffRegistrationDto(string FullName, string Email, string Phone, string Password);
public record UpdateStaffRoleDto(Guid UserId, RoleType NewRole);
public record UpdateStaffDetailsDto(Guid UserId, string FullName, string Email, string Phone);

public record AddPartDto(string Name, string PartNumber, decimal UnitPrice, int QuantityInStock, Guid VendorId, string Category);
public record UpdatePartDto(string Name, string PartNumber, decimal UnitPrice, int QuantityInStock, Guid VendorId, string Category);

public record PartListItemDto(
    Guid Id,
    string Name,
    string PartNumber,
    decimal UnitPrice,
    int QuantityInStock,
    Guid VendorId,
    string VendorName,
    string Category,
    bool IsLowStock);

public record AdminDashboardDto(
    decimal TodaySales,
    int TodayInvoiceCount,
    int LowStockPartCount,
    int OverdueCreditCount,
    decimal TotalPendingCredit,
    int CustomerCount,
    int ActiveStaffCount,
    int PendingPartRequests);
public record PurchasePartDto(Guid VendorId, decimal TotalAmount);

public record PartRequestAdminDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    string PartName,
    string? Description,
    string Status);

public record UpdatePartRequestStatusDto(string Status);

public record PagedPartsResultDto(IReadOnlyList<PartListItemDto> Items, int TotalCount, int Page, int PageSize);
