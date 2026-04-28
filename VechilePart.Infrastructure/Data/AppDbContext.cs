using Microsoft.EntityFrameworkCore;
using VechilePart.Domain.Entities;

namespace VechilePart.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<ServiceReview> ServiceReviews => Set<ServiceReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Columns not present in current migrations — keep client-only until a migration adds them.
        modelBuilder.Entity<PartRequest>().Ignore(p => p.RequestedAtUtc);
        modelBuilder.Entity<ServiceReview>().Ignore(r => r.CreatedAtUtc);
    }
}
