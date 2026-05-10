using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace VehiclePart.Application.Services;

public class EmailInvoiceService(INotificationService notificationService) : IEmailInvoiceService
{
    public async Task<bool> SendInvoiceEmailAsync(EmailInvoiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerEmail))
            return false;

        var subject = $"Invoice #{dto.InvoiceId}";
        var body = $"""
        <h2>Vehicle Parts Invoice</h2>
        <p>Hello {dto.CustomerName},</p>
        <ul>
            <li>Total Amount: {dto.TotalAmount}</li>
            <li>Paid Amount: {dto.PaidAmount}</li>
            <li>Pending Credit: {dto.PendingCredit}</li>
        </ul>
        <p>Thank you.</p>
        """;

        await notificationService.SendEmailAsync(dto.CustomerEmail, subject, body);
        return true;
    }
}
