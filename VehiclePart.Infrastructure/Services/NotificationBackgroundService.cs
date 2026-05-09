using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Enums;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Services;

public class NotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Notification background cycle failed.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task ProcessNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var lowStockParts = await dbContext.Parts
            .Where(part => part.QuantityInStock < 10)
            .ToListAsync(cancellationToken);

        if (lowStockParts.Count > 0)
        {
            var adminEmails = await dbContext.Users
                .Where(user => user.Role == RoleType.Admin && !string.IsNullOrWhiteSpace(user.Email))
                .Select(user => user.Email)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (adminEmails.Count > 0)
            {
                var lowStockSummary = string.Join(", ", lowStockParts.Select(part => $"{part.Name} ({part.QuantityInStock})"));
                foreach (var adminEmail in adminEmails)
                {
                    await notificationService.SendEmailAsync(
                        adminEmail,
                        "Low stock alert",
                        $"The following parts are below stock threshold (<10): {lowStockSummary}");
                }
            }
        }

        var overdueThreshold = DateTime.UtcNow.AddMonths(-1);
        var overdueCredits = await (
            from invoice in dbContext.SalesInvoices
            join customer in dbContext.Customers on invoice.CustomerId equals customer.Id
            where invoice.PendingCredit > 0
                  && invoice.IssuedAtUtc <= overdueThreshold
                  && !string.IsNullOrWhiteSpace(customer.Email)
            select new
            {
                customer.Email,
                invoice.Id,
                invoice.PendingCredit,
                invoice.IssuedAtUtc
            })
            .ToListAsync(cancellationToken);

        foreach (var overdue in overdueCredits)
        {
            await notificationService.SendEmailAsync(
                overdue.Email!,
                "Payment reminder for overdue credit",
                $"Invoice {overdue.Id} has pending credit {overdue.PendingCredit} since {overdue.IssuedAtUtc:yyyy-MM-dd}. Please clear your dues.");
        }
    }
}
