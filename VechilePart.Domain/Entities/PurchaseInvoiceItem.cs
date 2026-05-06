namespace VehiclePart.Domain.Entities;

public class PurchaseInvoiceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PurchaseInvoiceId { get; set; }

    public PurchaseInvoice? PurchaseInvoice { get; set; }

    public Guid PartId { get; set; }

    public Part? Part { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}