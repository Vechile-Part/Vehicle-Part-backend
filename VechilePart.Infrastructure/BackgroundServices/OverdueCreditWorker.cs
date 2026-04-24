using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VechilePart.Application.Interfaces;
using VechilePart.Infrastructure.Services;

namespace VechilePart.Infrastructure.BackgroundServices;

public class OverdueCreditWorker(
    IServiceProvider serviceProvider,
    ILogger<OverdueCreditWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Background Job: Checking for overdue credit balances...");

            using (var scope = serviceProvider.CreateScope())
            {
                var customerRepo = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // In a real database, we would query customers with unpaid invoices > 1 month
                // For this scenario, we simulate finding one such customer
                var customers = await customerRepo.GetAllCustomersAsync(stoppingToken);
                foreach (var customer in customers)
                {
                    // Mock condition: If phone ends in '9', they have overdue credit (for demo)
                    if (customer.Phone.EndsWith("9"))
                    {
                        await notificationService.SendOverdueCreditReminderAsync(customer, 150.00m);
                    }
                }
            }

            // Run once every hour (or every minute for demo testing if needed)
            await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
        }
    }
}
