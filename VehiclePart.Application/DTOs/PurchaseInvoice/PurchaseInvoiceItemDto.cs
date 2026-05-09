namespace VehiclePart.Application.DTOs.PurchaseInvoice;

public class PurchaseInvoiceItemDto
{
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}