using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;


public interface ICustomerRepository
{
    Task<Customer> AddCustomerAsync(Customer customer, CancellationToken ct);
    Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken ct);
    Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<Customer>> GetAllCustomersAsync(CancellationToken ct = default);
    Task UpdateCustomerAsync(Customer customer, CancellationToken ct);
    Task AddVehicleAsync(Vehicle vehicle, CancellationToken ct);
    Task<List<Vehicle>> GetVehiclesByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task UpdateVehicleAsync(Vehicle vehicle, CancellationToken ct);
    Task<Vehicle?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken ct);
    Task DeleteVehicleAsync(Guid customerId, Guid vehicleId, CancellationToken ct);

    Task AddAppointmentAsync(Appointment appointment, CancellationToken ct);
    Task<bool> TryAddAppointmentAsync(Appointment appointment, CancellationToken ct);
    Task<IReadOnlyList<Appointment>> GetAppointmentsByCustomerIdAsync(Guid customerId, CancellationToken ct);
    Task<Appointment?> GetAppointmentForCustomerAsync(Guid customerId, Guid appointmentId, CancellationToken ct);
    Task<IReadOnlyList<Appointment>> GetReviewableAppointmentsAsync(Guid customerId, CancellationToken ct);
    Task<bool> HasServiceReviewAsync(Guid customerId, Guid serviceId, CancellationToken ct);
    Task<bool> HasAppointmentAtUtcAsync(DateTime appointmentDateUtc, CancellationToken ct);
    Task<IReadOnlyList<DateTime>> GetAppointmentTimesForNepalLocalDayAsync(int year, int month, int day, CancellationToken ct);
    Task<IReadOnlyList<StaffAppointmentDto>> GetStaffAppointmentsAsync(CancellationToken ct);
    Task<Appointment?> GetAppointmentByIdForUpdateAsync(Guid appointmentId, CancellationToken ct);
    Task UpdateAppointmentAsync(Appointment appointment, CancellationToken ct);
    Task AddPartRequestAsync(PartRequest partRequest, CancellationToken ct);
    Task AddServiceReviewAsync(ServiceReview review, CancellationToken ct);

    Task<List<SalesInvoice>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken ct);

    Task AddCustomerPasswordSetupTokenAsync(CustomerPasswordSetupToken token, CancellationToken ct);
    Task<CustomerPasswordSetupToken?> GetActivePasswordSetupTokenByHashAsync(string tokenHash, CancellationToken ct);
    Task MarkPasswordSetupTokenUsedAsync(Guid tokenId, CancellationToken ct);
    Task InvalidateUnusedPasswordSetupTokensForCustomerAsync(Guid customerId, CancellationToken ct);
    Task SetCustomerPasswordHashAsync(Guid customerId, string passwordHash, CancellationToken ct);
}