namespace VehiclePart.Application.DTOs;
using VehiclePart.Domain.Enums;

public record FinancialReportDto(string ReportType, decimal TotalSales, decimal TotalPurchases, decimal PendingCredits);

public record StaffRegistrationDto(string FullName, string Email, string Phone);
public record UpdateStaffRoleDto(Guid UserId, RoleType NewRole);
public record AddPartDto(string Name, string PartNumber, decimal UnitPrice, int QuantityInStock, Guid VendorId);
public record UpdatePartDto(string Name, string PartNumber, decimal UnitPrice, int QuantityInStock, Guid VendorId);
public record PurchasePartDto(Guid VendorId, decimal TotalAmount);
