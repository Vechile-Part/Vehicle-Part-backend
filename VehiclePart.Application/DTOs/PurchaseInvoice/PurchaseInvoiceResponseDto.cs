namespace VehiclePart.Application.DTOs.PurchaseInvoice;

public class PurchaseInvoiceResponseDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseInvoiceItemDto> Items { get; set; } = [];
}