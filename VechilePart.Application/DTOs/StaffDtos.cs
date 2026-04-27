namespace VechilePart.Application.DTOs;

public record CustomerRegistrationDto(string FullName, string Phone, string Email, string GovernmentId, string VehicleNumber, string Make, string Model, int Year);
public record SalesInvoiceCreateDto(Guid CustomerId, decimal TotalAmount, decimal PaidAmount);
public record CustomerSearchDto(string? VehicleNumber, string? Phone, string? GovernmentId, string? FullName);
public record CustomerReportDto(int RegularCustomers, int HighSpenders, int CustomersWithPendingCredits);
