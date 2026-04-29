namespace VechilePart.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string GovernmentId { get; set; } = string.Empty; 
    public List<Vehicle> Vehicles { get; set; } = new(); 
}