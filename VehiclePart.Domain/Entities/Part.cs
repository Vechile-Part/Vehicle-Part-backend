namespace VehiclePart.Domain.Entities;

public class Part
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string PartNumber { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int QuantityInStock { get; set; }
    public Guid VendorId { get; set; }

    public uint RowVersion { get; set; }
}
