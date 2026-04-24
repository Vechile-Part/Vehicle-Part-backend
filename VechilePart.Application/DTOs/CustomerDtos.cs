namespace VechilePart.Application.DTOs;

public record CustomerSelfRegistrationDto(string FullName, string Phone, string Email, string GovernmentId);
public record VehicleDto(Guid Id, string VehicleNumber, string Make, string Model, int Year);
public record AppointmentDto(Guid CustomerId, DateTime AppointmentAtUtc, string Notes);
public record PartRequestDto(Guid CustomerId, string PartName, string Notes);
public record ServiceReviewDto(Guid CustomerId, int Rating, string Comment);
public record CustomerProfileDto(Guid Id, string FullName, string Phone, string Email, string GovernmentId);
