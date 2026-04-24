using Microsoft.Extensions.Logging;
using VechilePart.Domain.Entities;
using VechilePart.Application.Interfaces;

namespace VechilePart.Infrastructure.Services;

public class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public Task NotifyStockLowAsync(Part part)
    {
        logger.LogWarning("ADMIN ALERT: Stock for part {PartName} (ID: {PartId}) has fallen below 10 units. Current stock: {Stock}", 
            part.Name, part.Id, part.QuantityInStock);
        return Task.CompletedTask;
    }

    public Task SendOverdueCreditReminderAsync(Customer customer, decimal balance)
    {
        logger.LogInformation("EMAIL SENT to {Email}: Reminder: You have an unpaid credit balance of ${Balance} overdue by more than one month.", 
            customer.Email, balance);
        return Task.CompletedTask;
    }
}
