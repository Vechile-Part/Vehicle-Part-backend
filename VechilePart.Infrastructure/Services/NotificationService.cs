using Microsoft.Extensions.Logging;
using VechilePart.Application.Interfaces;

namespace VechilePart.Infrastructure.Services;

public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        logger.LogInformation("EMAIL SENT to {Email}: {Subject} - {Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
