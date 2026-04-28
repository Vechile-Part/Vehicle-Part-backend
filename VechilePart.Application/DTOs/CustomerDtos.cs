namespace VechilePart.Application.DTOs;

public record CustomerSelfRegistrationDto(string FullName, string Phone, string Email, string Password);
public record VehicleDto(Guid Id, string VehicleNumber, string Make, string Model, int Year);
public record AppointmentDto(Guid CustomerId, DateTime AppointmentAtUtc, string Notes);
public record PartRequestDto(Guid CustomerId, string PartName, string Notes);
public record ServiceReviewDto(Guid CustomerId, int Rating, string Comment);
public record CustomerProfileDto(Guid Id, string FullName, string Phone, string Email);
public record VehicleHealthInsight(string PartName, double RiskLevel, string Recommendation, string DaysRemaining);

public record AppointmentResponseDto(Guid Id, Guid CustomerId, DateTime AppointmentAtUtc, string Notes);
public record PartRequestResponseDto(Guid Id, Guid CustomerId, string PartName, string Notes, DateTime RequestedAtUtc);
public record ServiceReviewResponseDto(Guid Id, Guid CustomerId, int Rating, string Comment, DateTime CreatedAtUtc);
public record PurchaseHistoryDto(Guid Id, DateTime IssuedAtUtc, decimal TotalAmount, decimal DiscountAmount, decimal PaidAmount, decimal PendingCredit);
