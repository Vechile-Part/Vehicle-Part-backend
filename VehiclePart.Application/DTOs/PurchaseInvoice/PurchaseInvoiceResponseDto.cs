namespace VehiclePart.Application.DTOs.PurchaseInvoice;

public class PurchaseInvoiceResponseDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string VendorContactPerson { get; set; } = string.Empty;
    public string VendorPhone { get; set; } = string.Empty;
    public string VendorEmail { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchaseInvoiceItemDto> Items { get; set; } = [];
}