using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VehiclePart.Application.Interfaces;


namespace VehiclePart.Infrastructure.Services;

public class NotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<NotificationBackgroundService> logger) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalMinutes = configuration.GetValue("NotificationSettings:CheckIntervalMinutes", 60);
        var interval = TimeSpan.FromMinutes(Math.Max(5, intervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var runner = scope.ServiceProvider.GetRequiredService<INotificationJobRunner>();
                var result = await runner.RunAsync(forceBypassCooldowns: false, stoppingToken);
                logger.LogInformation(
                    "Notification cycle: lowStock={LowStock}, lowStockEmails={LowStockEmails}, creditReminders={CreditReminders}",
                    result.LowStockPartCount,
                    result.LowStockEmailsSent,
                    result.CreditRemindersSent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Notification background cycle failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
