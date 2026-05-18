namespace VehiclePart.Application.DTOs;

public record StaffCustomerSearchRow(
    Guid Id,
    string FullName,
    string Phone,
    string Email,
    string VehicleNumber,
    string Make,
    string Model,
    int Year);
