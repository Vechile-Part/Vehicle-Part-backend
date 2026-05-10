namespace VehiclePart.Application.DTOs;

public record CustomerSelfRegistrationDto(string FullName, string Phone, string Email, string Password);
public record VehicleDto(Guid Id, string VehicleNumber, string Make, string Model, int Year);
public record CustomerProfileDto(Guid Id, string FullName, string Phone, string Email, string? ProfilePictureUrl = null);
public record VehicleHealthInsight(string PartName, double RiskLevel, string Recommendation, string DaysRemaining);

public record BookAppointmentDto(DateTime AppointmentDate, string ServiceType, string? Notes);
public record PartRequestDto(string PartName, string? Description);
public record ServiceReviewDto(Guid ServiceId, int Rating, string? Comment);

public record PurchaseHistoryDto(Guid Id, decimal TotalAmount, decimal PaidAmount, decimal PendingCredit, DateTime IssuedAtUtc);

public record AppointmentDto(Guid Id, DateTime AppointmentDate, string ServiceType, string Status, string? Notes);