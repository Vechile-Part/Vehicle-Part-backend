using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface IAuthService
{
    Task<AuthLoginResult<StaffLoginResponse>> LoginStaffAsync(
        string? email,
        string password,
        CancellationToken cancellationToken = default);

    Task<AuthLoginResult<CustomerLoginResponse>> LoginCustomerAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
