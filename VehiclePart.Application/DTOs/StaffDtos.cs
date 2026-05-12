namespace VehiclePart.Application.DTOs;

public record CustomerRegistrationDto(string FullName, string Phone, string Email, string Password, string VehicleNumber, string Make, string Model, int Year);


public record SalesInvoiceLineItemDto(Guid PartId, int Quantity);
public record SalesInvoiceCreateDto(Guid CustomerId, decimal PaidAmount, decimal DiscountAmount, List<SalesInvoiceLineItemDto> Items);

public record SalesInvoiceItemResponseDto(Guid Id, Guid PartId, string PartName, int Quantity, decimal UnitPrice, decimal LineTotal);
public record SalesInvoiceResponseDto(
    Guid Id,
    Guid CustomerId,
    DateTime IssuedAtUtc,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal PaidAmount,
    decimal PendingCredit,
    List<SalesInvoiceItemResponseDto> Items);


public record CustomerSearchDto(string? VehicleNumber, string? Phone, string? FullName, Guid? CustomerId);

public record CustomerReportDto(int RegularCustomers, int HighSpenders, int CustomersWithPendingCredits);