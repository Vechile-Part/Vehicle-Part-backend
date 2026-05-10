using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface ICustomerAuthService
{
    Task<AuthenticatedCustomer?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
}
