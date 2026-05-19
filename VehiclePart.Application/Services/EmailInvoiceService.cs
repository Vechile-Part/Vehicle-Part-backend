using VehiclePart.Application.DTOs;
using VehiclePart.Application.Formatting;
using VehiclePart.Application.Interfaces;

namespace VehiclePart.Application.Services;

public class EmailInvoiceService(INotificationService notificationService) : IEmailInvoiceService
{
    public async Task<bool> SendInvoiceEmailAsync(EmailInvoiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerEmail))
            return false;

        var subject = $"Sales invoice · #{dto.InvoiceId}";
        var body = SalesInvoiceEmailTemplate.BuildSummaryOnly(
            dto.CustomerName,
            $"#{dto.InvoiceId}",
            dto.TotalAmount,
            dto.PaidAmount,
            dto.PendingCredit);

        await notificationService.SendEmailAsync(dto.CustomerEmail, subject, body);
        return true;
    }
}
