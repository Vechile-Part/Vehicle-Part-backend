namespace VehiclePart.Application.DTOs;

/// <summary>
/// Sales invoice with open balance older than the reminder policy window (admin snapshot).
/// </summary>
public record OverdueCreditInvoiceDto(
    Guid InvoiceId,
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    decimal PendingCredit,
    DateTime IssuedAtUtc,
    DateTime? LastReminderSentUtc);
