using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Security;
using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;

namespace VehiclePart.Application.Services;

public sealed class CustomerInviteService(
    ICustomerRepository repository,
    IAdminRepository adminRepository,
    INotificationService notificationService,
    IConfiguration configuration,
    ILogger<CustomerInviteService> logger
) : ICustomerInviteService
{
    private static readonly TimeSpan InviteTtl = TimeSpan.FromHours(72);

    public async Task SendPasswordSetupInviteAsync(Guid customerId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        await repository.InvalidateUnusedPasswordSetupTokensForCustomerAsync(customerId, cancellationToken);

        var rawToken = InviteTokenHasher.CreateRawToken();
        var tokenHash = InviteTokenHasher.ComputeStorageHash(rawToken);

        await repository.AddCustomerPasswordSetupTokenAsync(new CustomerPasswordSetupToken
        {
            CustomerId = customerId,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTime.UtcNow.Add(InviteTtl),
        }, cancellationToken);

        var baseUrl = (configuration["App:CustomerPortalBaseUrl"] ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            baseUrl = "http://localhost:3000";

        var link = $"{baseUrl}/auth/set-password?token={Uri.EscapeDataString(rawToken)}";
        var subject = "Set your account password";
        var body = $"""
            <p>Hello,</p>
            <p>Your vehicle parts account has been created by our team. Use the link below to choose your password and sign in.</p>
            <p><a href="{link}">Set password</a></p>
            <p>If the button does not work, copy this address into your browser:</p>
            <p style="word-break:break-all">{link}</p>
            <p>This link expires in 72 hours.</p>
            """;

        var sent = await notificationService.TrySendEmailAsync(normalizedEmail, subject, body, cancellationToken);
        if (!sent)
            logger.LogWarning("Password setup invite for customer {CustomerId} could not be emailed to {Email}.", customerId, normalizedEmail);
    }

    public async Task<(bool Ok, string? Error)> TryCompletePasswordInviteAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, "Token is required.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return (false, "Password must be at least 8 characters.");

        var hash = InviteTokenHasher.ComputeStorageHash(token.Trim());
        var row = await repository.GetActivePasswordSetupTokenByHashAsync(hash, cancellationToken);
        if (row is null)
            return (false, "This link is invalid or has expired.");

        var customer = await repository.GetCustomerAsync(row.CustomerId, cancellationToken);
        if (customer is null)
            return (false, "Account was not found.");

        var newHash = CustomerPasswordHasher.HashPassword(newPassword);
        await repository.SetCustomerPasswordHashAsync(customer.Id, newHash, cancellationToken);
        await repository.MarkPasswordSetupTokenUsedAsync(row.Id, cancellationToken);
        return (true, null);
    }

    public async Task SendStaffPasswordSetupInviteAsync(
        Guid userId,
        string email,
        string fullName,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        await adminRepository.InvalidateUnusedPasswordSetupTokensForUserAsync(userId, cancellationToken);

        var rawToken = InviteTokenHasher.CreateRawToken();
        var tokenHash = InviteTokenHasher.ComputeStorageHash(rawToken);

        await adminRepository.AddUserPasswordSetupTokenAsync(new UserPasswordSetupToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTime.UtcNow.Add(InviteTtl),
        }, cancellationToken);

        var baseUrl = (configuration["App:CustomerPortalBaseUrl"] ?? "").TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
            baseUrl = "http://localhost:3000";

        var link = $"{baseUrl}/auth/set-password?token={Uri.EscapeDataString(rawToken)}&kind=staff";
        var displayName = string.IsNullOrWhiteSpace(fullName) ? "there" : fullName.Trim();
        var subject = "Set your staff account password";
        var body = $"""
            <p>Hello {displayName},</p>
            <p>An administrator created your PartTrack staff account. Use the link below to choose your password and sign in.</p>
            <p><a href="{link}">Set password</a></p>
            <p>If the button does not work, copy this address into your browser:</p>
            <p style="word-break:break-all">{link}</p>
            <p>This link expires in 72 hours.</p>
            """;

        var sent = await notificationService.TrySendEmailAsync(normalizedEmail, subject, body, cancellationToken);
        if (!sent)
            logger.LogWarning("Staff password setup invite for user {UserId} could not be emailed to {Email}.", userId, normalizedEmail);
    }

    public async Task<(bool Ok, string? Error)> TryCompleteStaffPasswordInviteAsync(
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, "Token is required.");

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            return (false, "Password must be at least 8 characters.");

        var hash = InviteTokenHasher.ComputeStorageHash(token.Trim());
        var row = await adminRepository.GetActiveUserPasswordSetupTokenByHashAsync(hash, cancellationToken);
        if (row is null)
            return (false, "This link is invalid or has expired.");

        var user = await adminRepository.GetUserByIdAsync(row.UserId, cancellationToken);
        if (user is null)
            return (false, "Account was not found.");

        if (user.Role is not RoleType.Staff and not RoleType.Admin)
            return (false, "This link is not valid for staff sign-in.");

        var newHash = CustomerPasswordHasher.HashPassword(newPassword);
        await adminRepository.SetUserPasswordHashAsync(user.Id, newHash, cancellationToken);
        await adminRepository.MarkUserPasswordSetupTokenUsedAsync(row.Id, cancellationToken);
        return (true, null);
    }
}
