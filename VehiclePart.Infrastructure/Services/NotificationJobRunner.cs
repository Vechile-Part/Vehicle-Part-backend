using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Application.Formatting;
using VehiclePart.Domain.Enums;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Services;

public class NotificationJobRunner(
    AppDbContext dbContext,
    INotificationService notificationService,
    IConfiguration configuration,
    ILogger<NotificationJobRunner> logger) : INotificationJobRunner
{
    private const int LowStockThreshold = 10;
    private const int CreditOverdueMonths = 1;

    public async Task<NotificationJobResult> RunAsync(
        bool forceBypassCooldowns = false,
        CancellationToken cancellationToken = default)
    {
        var messages = new List<string>();
        var smtpConfigured = IsSmtpConfigured();

        if (!smtpConfigured)
            messages.Add("SMTP is not fully configured; emails were not sent.");

        await EnsureNotificationJobStateAsync(cancellationToken);

        var lowStockCount = await ProcessLowStockDigestEmailsAsync(forceBypassCooldowns, messages, cancellationToken);
        var creditSent = await ProcessOverdueCreditRemindersAsync(forceBypassCooldowns, messages, cancellationToken);

        return new NotificationJobResult(
            lowStockCount.Parts,
            lowStockCount.EmailsSent,
            creditSent,
            smtpConfigured,
            messages);
    }

    private bool IsSmtpConfigured()
    {
        return !string.IsNullOrWhiteSpace(configuration["EmailSettings:SmtpServer"])
               && !string.IsNullOrWhiteSpace(configuration["EmailSettings:Port"])
               && !string.IsNullOrWhiteSpace(configuration["EmailSettings:SenderEmail"])
               && !string.IsNullOrWhiteSpace(configuration["EmailSettings:Username"])
               && !string.IsNullOrWhiteSpace(configuration["EmailSettings:Password"]);
    }

    private TimeSpan LowStockDigestCooldown(bool force) =>
        force ? TimeSpan.Zero : TimeSpan.FromHours(configuration.GetValue("NotificationSettings:LowStockDigestCooldownHours", 24));

    private TimeSpan CreditReminderCooldown(bool force) =>
        force ? TimeSpan.Zero : TimeSpan.FromDays(configuration.GetValue("NotificationSettings:CreditReminderCooldownDays", 28));

    private async Task EnsureNotificationJobStateAsync(CancellationToken cancellationToken)
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

    private bool SendLowStockDigestEmail() =>
        configuration.GetValue("NotificationSettings:SendLowStockDigestEmail", true);

    private async Task<(int Parts, int EmailsSent)> ProcessLowStockDigestEmailsAsync(
        bool force,
        List<string> messages,
        CancellationToken cancellationToken)
    {
        var lowStockParts = await dbContext.Parts
            .Where(part => part.QuantityInStock < LowStockThreshold)
            .ToListAsync(cancellationToken);

        if (lowStockParts.Count == 0)
        {
            messages.Add("No parts below stock threshold.");
            return (0, 0);
        }

        if (!SendLowStockDigestEmail())
        {
            messages.Add(
                $"{lowStockParts.Count} part(s) below threshold; admin is notified on the Stock & credit alerts page (low-stock email is disabled in config).");
            return (lowStockParts.Count, 0);
        }

        var state = await dbContext.NotificationJobStates
            .FirstAsync(s => s.Id == NotificationJobState.WellKnownId, cancellationToken);

        var cooldown = LowStockDigestCooldown(force);
        if (state.LastLowStockDigestSentUtc is DateTime last && DateTime.UtcNow - last < cooldown)
        {
            messages.Add($"Low-stock digest skipped (cooldown until {last.Add(cooldown):u} UTC). Use force=true to send now.");
            return (lowStockParts.Count, 0);
        }

        var recipients = await GetAdminEmailsAsync(cancellationToken);
        if (recipients.Count == 0)
        {
            messages.Add(
                "Low-stock digest skipped: no admin email addresses found. " +
                "Ensure at least one user with the Admin role has a valid email.");
            return (lowStockParts.Count, 0);
        }

        var senderEmail = configuration["EmailSettings:SenderEmail"] ?? "admin mailbox";
        var lowStockSummary = string.Join(", ", lowStockParts.Select(part => $"{part.Name} ({part.QuantityInStock})"));
        const string subject = "Low stock alert";
        var body =
            $"""
            <p>The following parts are below stock threshold (&lt;{LowStockThreshold}):</p>
            <p>{System.Net.WebUtility.HtmlEncode(lowStockSummary)}</p>
            <p><em>Sent from {System.Net.WebUtility.HtmlEncode(senderEmail)} (admin notification account).</em></p>
            """;

        var sentCount = 0;
        foreach (var email in recipients)
        {
            if (await notificationService.TrySendEmailAsync(email, subject, body, cancellationToken))
                sentCount++;
        }

        if (sentCount > 0)
        {
            state.LastLowStockDigestSentUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            messages.Add(
                $"Low-stock digest emailed to {sentCount} admin recipient(s), sent from admin account.");
            logger.LogInformation(
                "Low stock digest emailed to {Count} admin mailbox(es) from {Sender}.",
                sentCount,
                senderEmail);
        }
        else
        {
            messages.Add("Low-stock digest: no emails were sent (check SMTP settings).");
            logger.LogWarning("Low stock digest: {PartCount} parts below threshold but email send failed.", lowStockParts.Count);
        }

        return (lowStockParts.Count, sentCount);
    }

    private async Task<List<string>> GetAdminEmailsAsync(CancellationToken cancellationToken)
    {
        var emails = await dbContext.Users
            .Where(user => user.Role == RoleType.Admin && !string.IsNullOrWhiteSpace(user.Email))
            .Select(user => user.Email)
            .ToListAsync(cancellationToken);

        return emails
            .Select(email => email.Trim())
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<int> ProcessOverdueCreditRemindersAsync(
        bool force,
        List<string> messages,
        CancellationToken cancellationToken)
    {
        var overdueThreshold = DateTime.UtcNow.AddMonths(-CreditOverdueMonths);
        var cooldownCutoff = DateTime.UtcNow.Subtract(CreditReminderCooldown(force));

        var overdueInvoices = await (
            from invoice in dbContext.SalesInvoices.AsNoTracking()
            join customer in dbContext.Customers.AsNoTracking() on invoice.CustomerId equals customer.Id
            where invoice.PendingCredit > 0
                  && invoice.IssuedAtUtc <= overdueThreshold
                  && !string.IsNullOrWhiteSpace(customer.Email)
                  && (force || invoice.LastCreditReminderSentUtc == null || invoice.LastCreditReminderSentUtc < cooldownCutoff)
            select new
            {
                invoice.Id,
                invoice.InvoiceNumber,
                customer.Email,
                invoice.PendingCredit,
                invoice.IssuedAtUtc
            }
        ).ToListAsync(cancellationToken);

        if (overdueInvoices.Count == 0)
        {
            messages.Add("No overdue credit invoices eligible for customer reminders.");
            return 0;
        }

        var sentCount = 0;
        foreach (var row in overdueInvoices)
        {
            var invoice = await dbContext.SalesInvoices
                .FirstOrDefaultAsync(i => i.Id == row.Id, cancellationToken);
            if (invoice is null)
                continue;

            if (!force && invoice.LastCreditReminderSentUtc is DateTime sent && sent >= cooldownCutoff)
                continue;

            var invoiceRef = string.IsNullOrWhiteSpace(invoice.InvoiceNumber)
                ? invoice.Id.ToString()[..8].ToUpperInvariant()
                : invoice.InvoiceNumber;

            var subject = "Payment reminder for overdue credit";
            var body =
                $"""
                <h2>Vehicle Parts</h2>
                <p>Hello,</p>
                <p>Invoice <strong>{invoiceRef}</strong> has pending credit of <strong>{NprFormatter.Format(invoice.PendingCredit)}</strong> since <strong>{invoice.IssuedAtUtc:yyyy-MM-dd}</strong> (UTC).</p>
                <p>Please clear your balance at your earliest convenience.</p>
                <p>Thank you.</p>
                """;

            if (!await notificationService.TrySendEmailAsync(row.Email!, subject, body, cancellationToken))
                continue;

            invoice.LastCreditReminderSentUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            sentCount++;
            logger.LogInformation("Credit reminder sent for invoice {InvoiceId}.", invoice.Id);
        }

        messages.Add(sentCount > 0
            ? $"Credit reminders sent for {sentCount} invoice(s)."
            : $"Found {overdueInvoices.Count} overdue invoice(s) but no reminder emails were sent (check SMTP or customer emails).");

        return sentCount;
    }
}
