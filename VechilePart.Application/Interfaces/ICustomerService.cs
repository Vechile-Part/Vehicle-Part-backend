using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

public interface ICustomerService
{
    Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken cancellationToken = default);
    Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default);
    Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken cancellationToken = default);
}

public interface ICustomerFeatureService
{
    Task<AppointmentResponseDto> BookAppointmentAsync(BookAppointmentDto dto);
    Task<PartRequestResponseDto> RequestPartAsync(RequestPartDto dto);
    Task<ServiceReviewResponseDto> SubmitReviewAsync(SubmitReviewDto dto);
    Task<List<AppointmentResponseDto>> GetAppointmentsByCustomerAsync(Guid customerId);
    Task<List<PartRequestResponseDto>> GetPartRequestsByCustomerAsync(Guid customerId);
    Task<List<ServiceReviewResponseDto>> GetReviewsByCustomerAsync(Guid customerId);
    Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId);
}

public interface IAppDataStore
{
    List<Appointment> Appointments { get; }
    List<PartRequest> PartRequests { get; }
    List<ServiceReview> ServiceReviews { get; }
    List<SalesInvoice> SalesInvoices { get; }
    void Add<T>(T entity) where T : class;
    void SaveChanges();
}
