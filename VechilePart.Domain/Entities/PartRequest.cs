namespace VechilePart.Domain.Entities;

public class PartRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
