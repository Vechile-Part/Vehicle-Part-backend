using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace VehiclePart.Application.Services;

public class EmailInvoiceService : IEmailInvoiceService
{
    public async Task<bool> SendInvoiceEmailAsync(EmailInvoiceDto dto)
    {
        await Task.Delay(500);

        Console.WriteLine($"Invoice Email Sent To: {dto.CustomerEmail}");

        return true;
    }
}