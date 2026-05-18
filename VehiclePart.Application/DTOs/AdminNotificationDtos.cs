namespace VehiclePart.Application.DTOs;

public record OverdueCreditInvoiceDto(
    Guid InvoiceId,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    decimal PendingCredit,
    DateTime IssuedAtUtc,
    DateTime? LastReminderSentUtc);
