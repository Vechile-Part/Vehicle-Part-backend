namespace VehiclePart.Application.DTOs;

public class EmailInvoiceDto
{
    public int InvoiceId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PendingCredit { get; set; }
}