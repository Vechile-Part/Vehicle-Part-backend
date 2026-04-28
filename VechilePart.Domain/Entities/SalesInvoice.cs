namespace VehiclePart.Domain.Entities;

public class SalesInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingCredit { get; set; }
}
