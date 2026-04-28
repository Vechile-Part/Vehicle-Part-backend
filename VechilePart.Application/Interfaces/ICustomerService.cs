using VechilePart.Application.DTOs;

namespace VechilePart.Application.Interfaces;

public interface ICustomerService
{
    Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken ct);
    Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken ct);
    Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken ct);
    Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken ct);
    Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken ct);
    Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken ct);
    Task<IReadOnlyList<VehicleHealthInsight>> GetVehicleHealthAIAsync(Guid vehicleId, CancellationToken ct);

    // Feature 13
    Task BookAppointmentAsync(Guid customerId, BookAppointmentDto dto, CancellationToken ct);
    Task RequestPartAsync(Guid customerId, PartRequestDto dto, CancellationToken ct);
    Task ReviewServiceAsync(Guid customerId, ServiceReviewDto dto, CancellationToken ct);

    // Feature 14
    Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken ct);
}