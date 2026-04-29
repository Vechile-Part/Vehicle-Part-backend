using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Services;

public class CustomerService : ICustomerService {
    private readonly ICustomerRepository _repo;
    public CustomerService(ICustomerRepository repo) { _repo = repo; }

    public async Task RegisterCustomerAsync(CustomerRegistrationDto dto) {
        var customer = new Customer { FullName = dto.Name, Email = dto.Email, Phone = dto.Phone };
        await _repo.AddCustomerAsync(customer);

        var vehicle = new Vehicle { Model = dto.VehicleModel, PlateNumber = dto.VehiclePlateNumber, Customer = customer };
        await _repo.AddVehicleAsync(vehicle);
    }
}