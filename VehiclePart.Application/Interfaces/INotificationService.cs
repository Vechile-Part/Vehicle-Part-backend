using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;

public interface INotificationService
{
    Task SendEmailAsync(string to, string subject, string body);
}
