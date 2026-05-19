namespace VehiclePart.Application.DTOs;

public record CustomerRegistrationDto(string FullName, string Phone, string Email, string VehicleNumber, string Make, string Model, int Year);


public record SalesInvoiceLineItemDto(Guid PartId, int Quantity);
public record SalesInvoiceCreateDto(Guid CustomerId, decimal PaidAmount, decimal DiscountAmount, List<SalesInvoiceLineItemDto> Items);

public record SalesInvoiceItemResponseDto(Guid Id, Guid PartId, string PartName, int Quantity, decimal UnitPrice, decimal LineTotal);
public record SalesInvoiceResponseDto(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    DateTime IssuedAtUtc,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal PaidAmount,
    decimal PendingCredit,
    List<SalesInvoiceItemResponseDto> Items,
    bool EmailSent = false,
    string? EmailError = null);

public record SalesInvoiceSummaryDto(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    DateTime IssuedAtUtc,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal PaidAmount,
    decimal PendingCredit);


public record CustomerSearchDto(string? VehicleNumber, string? Phone, string? FullName, Guid? CustomerId);

public record CustomerReportRowDto(
    Guid CustomerId,
    string FullName,
    string Phone,
    string Email,
    int SalesInvoiceCount,
    decimal LifetimeSalesTotal,
    decimal LargestInvoiceTotal,
    decimal TotalOutstandingCredit);

public record CustomerReportDto(
    int RegularCustomers,
    int HighSpenders,
    int CustomersWithPendingCredits,
    IReadOnlyList<CustomerReportRowDto> RegularCustomerRows,
    IReadOnlyList<CustomerReportRowDto> HighSpenderRows,
    IReadOnlyList<CustomerReportRowDto> PendingCreditRows);

public record StaffAppointmentDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    string CustomerEmail,
    DateTime AppointmentDate,
    string ServiceType,
    string Status,
    string? Notes,
    Guid? VehicleId,
    string? VehicleNumber,
    string? VehicleMake,
    string? VehicleModel,
    int? VehicleYear);

public record UpdateAppointmentStatusDto(string Status);

public record StaffProfileDto(Guid Id, string FullName, string Email, string Phone);

public record UpdateStaffSelfProfileDto(string FullName, string Email, string Phone);

public record ChangeStaffPasswordDto(string CurrentPassword, string NewPassword);