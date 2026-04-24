namespace VechilePart.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public DateTime AppointmentAtUtc { get; set; }
    public string Notes { get; set; } = string.Empty;
}
