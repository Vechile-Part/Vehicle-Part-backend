
namespace VehiclePart.Application.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);

 
    Task<bool> TrySendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
