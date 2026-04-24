using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Services;

public class CustomerService(ICustomerRepository repository) : ICustomerService
{
    public async Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email
        }, cancellationToken);

        return customer.Id;
    }

    public Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddVehicleAsync(new Vehicle
        {
            CustomerId = customerId,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, cancellationToken);
    }

    public Task BookAppointmentAsync(AppointmentDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddAppointmentAsync(new Appointment
        {
            CustomerId = dto.CustomerId,
            AppointmentAtUtc = dto.AppointmentAtUtc,
            Notes = dto.Notes
        }, cancellationToken);
    }

    public Task RequestPartAsync(PartRequestDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddPartRequestAsync(new PartRequest
        {
            CustomerId = dto.CustomerId,
            PartName = dto.PartName,
            Notes = dto.Notes
        }, cancellationToken);
    }

    public Task AddServiceReviewAsync(ServiceReviewDto dto, CancellationToken cancellationToken = default)
    {
        return repository.AddServiceReviewAsync(new ServiceReview
        {
            CustomerId = dto.CustomerId,
            Rating = dto.Rating,
            Comment = dto.Comment
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<object>> GetPurchaseAndServiceHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var invoices = (await repository.GetSalesInvoicesAsync(cancellationToken)).Where(x => x.CustomerId == customerId).Select(x => (object)x);
        return invoices.ToList();
    }

    public async Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetCustomerAsync(customerId, cancellationToken);
        return customer == null ? null : new CustomerProfileDto(customer.Id, customer.FullName, customer.Phone, customer.Email);
    }

    public async Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken cancellationToken = default)
    {
        await repository.UpdateCustomerAsync(new Customer
        {
            Id = customerId,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var vehicles = await repository.GetVehiclesByCustomerIdAsync(customerId, cancellationToken);
        return vehicles.Select(v => new VehicleDto(v.Id, v.VehicleNumber, v.Make, v.Model, v.Year)).ToList();
    }

    public async Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default)
    {
        await repository.UpdateVehicleAsync(new Vehicle
        {
            Id = dto.Id,
            CustomerId = customerId,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<VehicleHealthInsight>> GetVehicleHealthAIAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        // AI Logic Simulation: Stub implementation for academic project
        var insights = new List<VehicleHealthInsight>
        {
            new("Brake Pads", 0.75, "High wear detected. Schedule replacement within 15 days.", "15 days"),
            new("Timing Belt", 0.30, "Condition normal. Inspect in 6 months.", "180 days"),
            new("Air Filter", 0.90, "Critical blockage. Replace immediately.", "2 days")
        };
        
        return await Task.FromResult(insights);
    }
}
