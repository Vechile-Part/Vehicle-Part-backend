namespace VechilePart.Application.DTOs;

public class BookAppointmentDto
{
    public Guid CustomerId { get; set; }
    public DateTime AppointmentAtUtc { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class RequestPartDto
{
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

public class SubmitReviewDto
{
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime AppointmentAtUtc { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class PartRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime RequestedAtUtc { get; set; }
}

public class ServiceReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public class PurchaseHistoryDto
{
    public Guid Id { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingCredit { get; set; }
}