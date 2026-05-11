namespace VehiclePart.Application.DTOs;

public record CreateVendorDto(
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string Address,
    string? CompanyRegistrationNumber = null);

public record UpdateVendorDto(
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string Address,
    string? CompanyRegistrationNumber,
    bool IsActive);

public record VendorDto(
    Guid Id,
    string Name,
    string ContactPerson,
    string Email,
    string Phone,
    string Address,
    string? CompanyRegistrationNumber,
    bool IsActive);
