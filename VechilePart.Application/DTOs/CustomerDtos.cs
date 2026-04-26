namespace VechilePart.Application.DTOs;

public record CustomerSelfRegistrationDto(string FullName, string Phone, string Email, string GovernmentId);
public record VehicleDto(Guid Id, string VehicleNumber, string Make, string Model, int Year);
public record CustomerProfileDto(Guid Id, string FullName, string Phone, string Email, string GovernmentId);
