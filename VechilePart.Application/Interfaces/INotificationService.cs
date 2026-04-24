using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface INotificationService
{
    Task NotifyStockLowAsync(Part part);
    Task SendOverdueCreditReminderAsync(Customer customer, decimal balance);
}
