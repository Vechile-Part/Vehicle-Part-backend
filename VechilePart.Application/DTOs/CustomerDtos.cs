namespace VechilePart.Application.DTOs;

public class CustomerRegistrationDto {
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string VehiclePlateNumber { get; set; } = string.Empty;
}