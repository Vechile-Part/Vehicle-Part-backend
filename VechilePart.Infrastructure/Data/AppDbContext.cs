using Microsoft.EntityFrameworkCore;
using VechilePart.Domain.Entities;
using VechilePart.Domain.Enums; 

namespace VechilePart.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Part> Parts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        modelBuilder.Entity<User>().HasData(new User 
        { 
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), 
            FullName = "Admin User", 
            Email = "admin@ride.com", 
            Role = RoleType.Admin 
        });
    }
}