using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface ICustomerService
{
    Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken cancellationToken = default);
    Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default);
    Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleHealthInsight>> GetVehicleHealthAIAsync(Guid vehicleId, CancellationToken cancellationToken = default);

    Task<Guid> BookAppointmentAsync(Guid customerId, AppointmentDto dto, CancellationToken cancellationToken = default);
    Task<Guid> RequestPartAsync(Guid customerId, PartRequestDto dto, CancellationToken cancellationToken = default);
    Task<Guid> SubmitServiceReviewAsync(Guid customerId, ServiceReviewDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppointmentResponseDto>> GetAppointmentsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartRequestResponseDto>> GetPartRequestsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ServiceReviewResponseDto>> GetReviewsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
}
