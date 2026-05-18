namespace VehiclePart.Application.DTOs.PurchaseInvoice;

public class PurchaseInvoiceItemDto
{
    public Guid PartId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}