using VehiclePart.Application.DTOs;

namespace VehiclePart.Application.Interfaces;

public interface ICustomerService
{
    Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken ct);
    Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken ct);
    Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken ct);
    Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken ct);
    Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken ct);
    Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken ct);
    Task DeleteVehicleAsync(Guid customerId, Guid vehicleId, CancellationToken ct);
    Task ChangePasswordAsync(Guid customerId, ChangeCustomerPasswordDto dto, CancellationToken ct);
    Task<IReadOnlyList<VehicleMaintenanceReminder>> GetVehicleMaintenanceRemindersAsync(
        Guid vehicleId,
        Guid customerId,
        CancellationToken ct);

    Task BookAppointmentAsync(Guid customerId, BookAppointmentDto dto, CancellationToken ct);

    Task<IReadOnlyList<DateTime>> GetBookedAppointmentTimesForDayAsync(int year, int month, int day, CancellationToken ct);

    Task<IReadOnlyList<AppointmentDto>> GetAppointmentsAsync(Guid customerId, CancellationToken ct);
    Task<IReadOnlyList<AppointmentDto>> GetReviewableAppointmentsAsync(Guid customerId, CancellationToken ct);
    Task RequestPartAsync(Guid customerId, PartRequestDto dto, CancellationToken ct);
    Task ReviewServiceAsync(Guid customerId, ServiceReviewDto dto, CancellationToken ct);

    Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken ct);
}