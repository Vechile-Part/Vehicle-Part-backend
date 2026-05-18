namespace VehiclePart.Application.Interfaces;

public interface ICustomerInviteService
{
    Task SendPasswordSetupInviteAsync(Guid customerId, string email, CancellationToken cancellationToken = default);
    Task<(bool Ok, string? Error)> TryCompletePasswordInviteAsync(string token, string newPassword, CancellationToken cancellationToken = default);
}
