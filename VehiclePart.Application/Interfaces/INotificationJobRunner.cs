namespace VehiclePart.Application.Interfaces;

public interface INotificationJobRunner
{
    Task<NotificationJobResult> RunAsync(bool forceBypassCooldowns = false, CancellationToken cancellationToken = default);
}

public sealed record NotificationJobResult(
    int LowStockPartCount,
    int LowStockEmailsSent,
    int CreditRemindersSent,
    bool SmtpConfigured,
    IReadOnlyList<string> Messages);
