using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken ct);
    Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken ct);
    Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken ct);
    Task UpdateCustomerAsync(Customer customer, CancellationToken ct);
    Task AddVehicleAsync(Vehicle vehicle, CancellationToken ct);
    Task<List<Vehicle>> GetVehiclesByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task UpdateVehicleAsync(Vehicle vehicle, CancellationToken ct);
    Task<Vehicle?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken ct);

    Task AddAppointmentAsync(Appointment appointment, CancellationToken ct);
    Task<IReadOnlyList<Appointment>> GetAppointmentsByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task AddPartRequestAsync(PartRequest partRequest, CancellationToken ct);
    Task AddServiceReviewAsync(ServiceReview review, CancellationToken ct);

    Task<List<SalesInvoice>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken ct);
}