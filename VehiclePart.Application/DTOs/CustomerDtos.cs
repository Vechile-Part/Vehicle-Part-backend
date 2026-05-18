namespace VehiclePart.Application.DTOs;

public record CustomerSelfRegistrationDto(string FullName, string Phone, string Email, string Password);
public record VehicleDto(Guid Id, string VehicleNumber, string Make, string Model, int Year);
public record CustomerProfileDto(Guid Id, string FullName, string Phone, string Email, string? ProfilePictureUrl = null);
public record VehicleMaintenanceReminder(
    string PartName,
    string Priority,
    string Recommendation,
    string SuggestedActionBy);

public record BookAppointmentDto(DateTime AppointmentDate, string ServiceType, string? Notes);
public record PartRequestDto(string PartName, string? Description);
public record ServiceReviewDto(Guid ServiceId, int Rating, string? Comment);

public record CompleteCustomerPasswordInviteDto(string Token, string NewPassword);
public record ChangeCustomerPasswordDto(string CurrentPassword, string NewPassword);

public record PurchaseHistoryDto(
    Guid Id,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal PaidAmount,
    decimal PendingCredit,
    DateTime IssuedAtUtc);

public record AppointmentDto(Guid Id, DateTime AppointmentDate, string ServiceType, string Status, string? Notes);