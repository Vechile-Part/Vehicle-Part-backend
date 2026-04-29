using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface ICustomerRepository {
    
    Task AddCustomerAsync(Customer customer);
    Task AddVehicleAsync(Vehicle vehicle);
}