using VechilePart.Domain.Entities;

namespace VechilePart.Infrastructure.Data;

public class AppDbContext
{
    public List<User> Users { get; } = [];
    public List<Customer> Customers { get; } = [];
    public List<Vehicle> Vehicles { get; } = [];
    public List<Vendor> Vendors { get; } = [];
    public List<Part> Parts { get; } = [];
    public List<PurchaseInvoice> PurchaseInvoices { get; } = [];
    public List<SalesInvoice> SalesInvoices { get; } = [];
    public List<Appointment> Appointments { get; } = [];
    public List<PartRequest> PartRequests { get; } = [];
    public List<ServiceReview> ServiceReviews { get; } = [];
}
