namespace VechilePart.Application.DTOs;

public record StaffRegistrationDto(string FullName, string Email, string Phone, string Role);
public record VendorDto(Guid Id, string Name, string ContactPerson, string Phone, string Email);
public record PartDto(Guid Id, string Name, string PartNumber, decimal UnitPrice, int QuantityInStock, Guid VendorId);
public record PurchaseInvoiceDto(Guid Id, Guid VendorId, decimal TotalAmount, DateTime IssuedAtUtc);
public record FinancialReportDto(string ReportType, decimal TotalSales, decimal TotalPurchases, decimal PendingCredits);
