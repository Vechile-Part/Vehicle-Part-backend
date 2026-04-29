using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface IStaffService {
    Task RegisterCustomerAsync(CustomerRegistrationDto dto); 
}