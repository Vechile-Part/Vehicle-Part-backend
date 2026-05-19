using System.Security.Claims;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Security;
using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;

namespace VehiclePart.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    ICustomerAuthService customerAuthService,
    IJwtTokenFactory jwtTokenFactory) : IAuthService
{
    public async Task<AuthLoginResult<StaffLoginResponse>> LoginStaffAsync(
        string? email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return AuthLoginResult<StaffLoginResponse>.Failure(
                "Email and password are required.",
                StatusCodes.Status400BadRequest);

        var normalizedEmail = email.Trim();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
            return AuthLoginResult<StaffLoginResponse>.Failure(
                "Invalid email or password.",
                StatusCodes.Status401Unauthorized);

        if (string.IsNullOrEmpty(user.Password))
            return AuthLoginResult<StaffLoginResponse>.Failure(
                "Set your password using the link sent to your email.",
                StatusCodes.Status401Unauthorized);

        if (!TryVerifyStaffPassword(user, password, out var mustRehash))
            return AuthLoginResult<StaffLoginResponse>.Failure(
                "Invalid email or password.",
                StatusCodes.Status401Unauthorized);

        if (!TryResolveStaffUserRole(user, out var resolvedRole))
            return AuthLoginResult<StaffLoginResponse>.Failure(
                "Invalid email or password.",
                StatusCodes.Status401Unauthorized);

        var passwordHash = mustRehash ? CustomerPasswordHasher.HashPassword(password) : user.Password;
        if (mustRehash || user.Role != resolvedRole)
            await userRepository.UpdateStaffCredentialsAsync(user.Id, passwordHash, resolvedRole, cancellationToken);

        var roleClaim = UserRoleClaimValue(resolvedRole);
        var token = jwtTokenFactory.CreateToken(
        [
            (ClaimTypes.Name, user.Email),
            (ClaimTypes.Role, roleClaim),
            ("UserId", user.Id.ToString())
        ]);

        return AuthLoginResult<StaffLoginResponse>.Success(new StaffLoginResponse(token, roleClaim));
    }

    public async Task<AuthLoginResult<CustomerLoginResponse>> LoginCustomerAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var auth = await customerAuthService.ValidateCredentialsAsync(email, password, cancellationToken);
        if (auth is null)
            return AuthLoginResult<CustomerLoginResponse>.Failure(
                "Invalid email or password.",
                StatusCodes.Status401Unauthorized);

        var token = jwtTokenFactory.CreateToken(
        [
            (ClaimTypes.Name, auth.Email),
            (ClaimTypes.Role, "Customer"),
            ("CustomerId", auth.Id.ToString())
        ]);

        return AuthLoginResult<CustomerLoginResponse>.Success(
            new CustomerLoginResponse(token, "Customer", auth.Id));
    }

    private static bool TryResolveStaffUserRole(User user, out RoleType resolved)
    {
        if (user.Role is RoleType.Admin or RoleType.Staff)
        {
            resolved = user.Role;
            return true;
        }

        if (string.Equals(user.Email, "admin.vehiclepart@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            resolved = RoleType.Admin;
            return true;
        }

        resolved = default;
        return false;
    }

    private static bool TryVerifyStaffPassword(User user, string password, out bool mustRehash)
    {
        mustRehash = false;
        if (CustomerPasswordHasher.LooksLikeHash(user.Password))
            return CustomerPasswordHasher.VerifyPassword(password, user.Password);

        if (user.Password == password)
        {
            mustRehash = true;
            return true;
        }

        return false;
    }

    private static string UserRoleClaimValue(RoleType role) =>
        role switch
        {
            RoleType.Admin => "Admin",
            RoleType.Staff => "Staff",
            _ => throw new InvalidOperationException($"Unexpected staff user role: {role}")
        };
}
