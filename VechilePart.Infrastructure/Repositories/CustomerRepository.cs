using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository {
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context) {
        _context = context;
    }

    // Task 6: Save Customer
    public async Task AddCustomerAsync(Customer customer) { 
        _context.Customers.Add(customer); 
        await _context.SaveChangesAsync(); 
    }

    // Task 6: Save Vehicle
    public async Task AddVehicleAsync(Vehicle vehicle) { 
        _context.Vehicles.Add(vehicle); 
        await _context.SaveChangesAsync(); 
    }
}