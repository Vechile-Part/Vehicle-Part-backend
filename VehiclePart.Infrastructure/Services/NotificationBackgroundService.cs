using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Services;

public class NotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    
    private const int CreditOverdueMonths = 1;

    private const int CreditReminderCooldownDays = 28;

    private static readonly TimeSpan LowStockDigestCooldown = TimeSpan.FromHours(24);

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

        await EnsureNotificationJobStateAsync(dbContext, cancellationToken);

        await ProcessLowStockAdminEmailsAsync(dbContext, notificationService, cancellationToken);

        await ProcessOverdueCreditRemindersAsync(dbContext, notificationService, cancellationToken);
    }

    private static async Task EnsureNotificationJobStateAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var exists = await dbContext.NotificationJobStates
            .AnyAsync(s => s.Id == NotificationJobState.WellKnownId, cancellationToken);
        if (exists)
            return;

        dbContext.NotificationJobStates.Add(new NotificationJobState());
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            
        }
    }

    private async Task ProcessLowStockAdminEmailsAsync(
        AppDbContext dbContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var lowStockParts = await dbContext.Parts
            .Where(part => part.QuantityInStock < 10)
            .ToListAsync(cancellationToken);

        if (lowStockParts.Count == 0)
            return;

        var state = await dbContext.NotificationJobStates
            .FirstAsync(s => s.Id == NotificationJobState.WellKnownId, cancellationToken);

        if (state.LastLowStockDigestSentUtc is DateTime last &&
            DateTime.UtcNow - last < LowStockDigestCooldown)
        {
            logger.LogDebug("Low stock digest skipped: last sent at {LastSentUtc}.", last);
            return;
        }

        var adminEmails = await dbContext.Users
            .Where(user => user.Role == RoleType.Admin && !string.IsNullOrWhiteSpace(user.Email))
            .Select(user => user.Email)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (adminEmails.Count == 0)
        {
            logger.LogWarning("Low stock digest skipped: no admin users with an email address.");
            return;
        }

        var lowStockSummary = string.Join(", ", lowStockParts.Select(part => $"{part.Name} ({part.QuantityInStock})"));
        const string subject = "Low stock alert";
        var body =
            $"<p>The following parts are below stock threshold (&lt;10):</p><p>{System.Net.WebUtility.HtmlEncode(lowStockSummary)}</p>";

        var anySent = false;
        foreach (var adminEmail in adminEmails)
        {
            if (string.IsNullOrWhiteSpace(adminEmail))
                continue;

            if (await notificationService.TrySendEmailAsync(adminEmail, subject, body, cancellationToken))
                anySent = true;
        }

        if (anySent)
        {
            state.LastLowStockDigestSentUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Low stock digest emailed to {Count} admin mailbox(es).", adminEmails.Count);
        }
    }

    private async Task ProcessOverdueCreditRemindersAsync(
        AppDbContext dbContext,
        INotificationService notificationService,
        CancellationToken cancellationToken)
    {
        var overdueThreshold = DateTime.UtcNow.AddMonths(-CreditOverdueMonths);
        var cooldownCutoff = DateTime.UtcNow.AddDays(-CreditReminderCooldownDays);

        var overdueInvoices = await (
            from invoice in dbContext.SalesInvoices.AsNoTracking()
            join customer in dbContext.Customers.AsNoTracking() on invoice.CustomerId equals customer.Id
            where invoice.PendingCredit > 0
                  && invoice.IssuedAtUtc <= overdueThreshold
                  && !string.IsNullOrWhiteSpace(customer.Email)
                  && (invoice.LastCreditReminderSentUtc == null || invoice.LastCreditReminderSentUtc < cooldownCutoff)
            select new { invoice.Id, customer.Email, invoice.PendingCredit, invoice.IssuedAtUtc }
        ).ToListAsync(cancellationToken);

        if (overdueInvoices.Count == 0)
            return;

        foreach (var row in overdueInvoices)
        {
            var invoice = await dbContext.SalesInvoices
                .FirstOrDefaultAsync(i => i.Id == row.Id, cancellationToken);
            if (invoice is null)
                continue;

            if (invoice.LastCreditReminderSentUtc is DateTime sent &&
                sent >= cooldownCutoff)
                continue;

            var subject = "Payment reminder for overdue credit";
            var body =
                $"""
                <h2>Vehicle Parts</h2>
                <p>Hello,</p>
                <p>Invoice <strong>{invoice.Id}</strong> has pending credit of <strong>{invoice.PendingCredit:C}</strong> since <strong>{invoice.IssuedAtUtc:yyyy-MM-dd}</strong> (UTC).</p>
                <p>Please clear your balance at your earliest convenience.</p>
                <p>Thank you.</p>
                """;

            if (!await notificationService.TrySendEmailAsync(row.Email!, subject, body, cancellationToken))
                continue;

            invoice.LastCreditReminderSentUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Credit reminder sent for invoice {InvoiceId} to customer email.", invoice.Id);
        }
    }
}
