using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
}
