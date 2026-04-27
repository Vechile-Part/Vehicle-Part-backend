using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Interfaces;

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