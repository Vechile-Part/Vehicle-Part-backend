using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Security;

namespace VehiclePart.Application.Services;

public sealed class CustomerAuthService(ICustomerRepository repository) : ICustomerAuthService
{
    public async Task<AuthenticatedCustomer?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetCustomerByEmailAsync(email, cancellationToken);
        if (customer is null || !CustomerPasswordHasher.VerifyPassword(password, customer.PasswordHash))
            return null;

        return new AuthenticatedCustomer(customer.Id, customer.Email);
    }
}
