namespace VehiclePart.Domain.Entities;

public class PurchaseInvoice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VendorId { get; set; }

    public Vendor? Vendor { get; set; }

    public DateTime IssuedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public List<PurchaseInvoiceItem> Items { get; set; } = [];
}