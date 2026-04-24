using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface ICustomerService
{
    Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken cancellationToken = default);
    Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default);
    Task BookAppointmentAsync(AppointmentDto dto, CancellationToken cancellationToken = default);
    Task RequestPartAsync(PartRequestDto dto, CancellationToken cancellationToken = default);
    Task AddServiceReviewAsync(ServiceReviewDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetPurchaseAndServiceHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default);
}
