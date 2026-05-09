namespace VehiclePart.Application.DTOs.PurchaseInvoice;

public class CreatePurchaseInvoiceDto
{
    public Guid VendorId { get; set; }
    public List<PurchaseInvoiceItemDto> Items { get; set; } = [];
}