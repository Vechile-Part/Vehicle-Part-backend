using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using VehiclePart.Application.Interfaces;

namespace VehiclePart.Infrastructure.Services;

public class NotificationService(
    IConfiguration configuration,
    ILogger<NotificationService> logger
) : INotificationService
{
    public async Task SendEmailAsync(string to, string subject, string body) =>
        _ = await TrySendEmailCoreAsync(to, subject, body, throwOnMissingConfig: true, CancellationToken.None);

    public Task<bool> TrySendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default) =>
        TrySendEmailCoreAsync(to, subject, body, throwOnMissingConfig: false, cancellationToken);

    private async Task<bool> TrySendEmailCoreAsync(
        string to,
        string subject,
        string body,
        bool throwOnMissingConfig,
        CancellationToken cancellationToken)
    {
        var smtpServer = configuration["EmailSettings:SmtpServer"];
        var portText = configuration["EmailSettings:Port"];
        var senderName = configuration["EmailSettings:SenderName"];
        var senderEmail = configuration["EmailSettings:SenderEmail"];
        var username = configuration["EmailSettings:Username"];
        var password =
            configuration["EmailSettings:Password"]
            ?? Environment.GetEnvironmentVariable("EMAIL_SETTINGS_PASSWORD");

        if (string.IsNullOrWhiteSpace(smtpServer) ||
            string.IsNullOrWhiteSpace(portText) ||
            string.IsNullOrWhiteSpace(senderEmail) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            if (throwOnMissingConfig)
                throw new InvalidOperationException("Email SMTP settings are missing in appsettings.json.");

            logger.LogWarning("Email skipped: SMTP settings are not configured.");
            return false;
        }

        try
        {
            var port = int.Parse(portText, System.Globalization.CultureInfo.InvariantCulture);

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName ?? "Vehicle Parts System", senderEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(username, password, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}", to);
            if (throwOnMissingConfig)
                throw;
            return false;
        }
    }
}
