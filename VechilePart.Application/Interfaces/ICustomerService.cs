using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface ICustomerService {
    Task RegisterCustomerAsync(CustomerRegistrationDto dto);
}