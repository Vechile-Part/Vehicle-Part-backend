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
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = configuration["EmailSettings:SmtpServer"];
        var portText = configuration["EmailSettings:Port"];
        var senderName = configuration["EmailSettings:SenderName"];
        var senderEmail = configuration["EmailSettings:SenderEmail"];
        var username = configuration["EmailSettings:Username"];
        var password = configuration["EmailSettings:Password"];

        if (string.IsNullOrWhiteSpace(smtpServer) ||
            string.IsNullOrWhiteSpace(portText) ||
            string.IsNullOrWhiteSpace(senderEmail) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Email SMTP settings are missing in appsettings.json.");
        }

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

        await smtp.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(username, password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);

        logger.LogInformation("Email sent successfully to {Email}", to);
    }
}
