using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface IEmailInvoiceService
{
    Task<bool> SendInvoiceEmailAsync(EmailInvoiceDto dto);
}