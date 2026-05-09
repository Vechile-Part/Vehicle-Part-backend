namespace VehiclePart.Application.DTOs;

public class CustomerHistoryDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public List<CustomerVehicleDto> Vehicles { get; set; } = [];
    public List<CustomerInvoiceDto> Invoices { get; set; } = [];
}

public class CustomerVehicleDto
{
    public Guid VehicleId { get; set; }
    public string VehicleNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
}

public class CustomerInvoiceDto
{
    public Guid InvoiceId { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal PendingCredit { get; set; }
}