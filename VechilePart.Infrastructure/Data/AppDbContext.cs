using Microsoft.EntityFrameworkCore;
using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;

namespace VechilePart.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDataStore
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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

    List<Appointment> IAppDataStore.Appointments => Appointments.ToList();
    List<PartRequest> IAppDataStore.PartRequests => PartRequests.ToList();
    List<ServiceReview> IAppDataStore.ServiceReviews => ServiceReviews.ToList();
    List<SalesInvoice> IAppDataStore.SalesInvoices => SalesInvoices.ToList();
    void IAppDataStore.Add<T>(T entity) => base.Add(entity);
    void IAppDataStore.SaveChanges() => base.SaveChanges();
}