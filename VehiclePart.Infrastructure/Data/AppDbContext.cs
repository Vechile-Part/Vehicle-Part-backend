using Microsoft.EntityFrameworkCore;
using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;

namespace VehiclePart.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<ServiceReview> ServiceReviews => Set<ServiceReview>();
    public DbSet<NotificationJobState> NotificationJobStates => Set<NotificationJobState>();
    public DbSet<CustomerPasswordSetupToken> CustomerPasswordSetupTokens => Set<CustomerPasswordSetupToken>();
    public DbSet<UserPasswordSetupToken> UserPasswordSetupTokens => Set<UserPasswordSetupToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PartRequest>().Ignore(p => p.RequestedAtUtc);
        modelBuilder.Entity<ServiceReview>().Ignore(r => r.CreatedAtUtc);
        modelBuilder.Entity<Part>().HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasOne<Vehicle>()
                .WithMany()
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<CustomerPasswordSetupToken>(e =>
        {
            e.HasIndex(t => t.TokenHash);
            e.HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserPasswordSetupToken>(e =>
        {
            e.HasIndex(t => t.TokenHash);
            e.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            FullName = "Admin",
            Email = "admin.vehiclepart@gmail.com",
            Phone = "9800000000",
            Password = "admin123@",
            Role = RoleType.Admin
        });
    }
}