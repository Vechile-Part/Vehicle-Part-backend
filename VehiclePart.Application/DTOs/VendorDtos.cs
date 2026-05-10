namespace VehiclePart.Application.DTOs;

public record CreateVendorDto(string Name, string ContactPerson, string Phone, string Email);
public record UpdateVendorDto(string Name, string ContactPerson, string Phone, string Email);
public record VendorDto(Guid Id, string Name, string ContactPerson, string Phone, string Email);
