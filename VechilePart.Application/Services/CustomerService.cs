using VechilePart.Application.DTOs;
using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;

namespace VechilePart.Application.Services;

public class CustomerFeatureService : ICustomerFeatureService
{
    private readonly IAppDataStore _dataStore;

    public CustomerFeatureService(IAppDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    public Task<AppointmentResponseDto> BookAppointmentAsync(BookAppointmentDto dto)
    {
        var appointment = new Appointment
        {
            CustomerId = dto.CustomerId,
            AppointmentAtUtc = dto.AppointmentAtUtc,
            Notes = dto.Notes
        };

        _dataStore.Add(appointment);
        _dataStore.SaveChanges();

        return Task.FromResult(new AppointmentResponseDto
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            AppointmentAtUtc = appointment.AppointmentAtUtc,
            Notes = appointment.Notes
        });
    }

    public Task<PartRequestResponseDto> RequestPartAsync(RequestPartDto dto)
    {
        var partRequest = new PartRequest
        {
            CustomerId = dto.CustomerId,
            PartName = dto.PartName,
            Notes = dto.Notes,
            RequestedAtUtc = DateTime.UtcNow
        };

        _dataStore.Add(partRequest);
        _dataStore.SaveChanges();

        return Task.FromResult(new PartRequestResponseDto
        {
            Id = partRequest.Id,
            CustomerId = partRequest.CustomerId,
            PartName = partRequest.PartName,
            Notes = partRequest.Notes,
            RequestedAtUtc = partRequest.RequestedAtUtc
        });
    }

    public Task<ServiceReviewResponseDto> SubmitReviewAsync(SubmitReviewDto dto)
    {
        var review = new ServiceReview
        {
            CustomerId = dto.CustomerId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dataStore.Add(review);
        _dataStore.SaveChanges();

        return Task.FromResult(new ServiceReviewResponseDto
        {
            Id = review.Id,
            CustomerId = review.CustomerId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAtUtc = review.CreatedAtUtc
        });
    }

    public Task<List<AppointmentResponseDto>> GetAppointmentsByCustomerAsync(Guid customerId)
    {
        var appointments = _dataStore.Appointments
            .Where(a => a.CustomerId == customerId)
            .Select(a => new AppointmentResponseDto
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                AppointmentAtUtc = a.AppointmentAtUtc,
                Notes = a.Notes
            }).ToList();

        return Task.FromResult(appointments);
    }

    public Task<List<PartRequestResponseDto>> GetPartRequestsByCustomerAsync(Guid customerId)
    {
        var requests = _dataStore.PartRequests
            .Where(r => r.CustomerId == customerId)
            .Select(r => new PartRequestResponseDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                PartName = r.PartName,
                Notes = r.Notes,
                RequestedAtUtc = r.RequestedAtUtc
            }).ToList();

        return Task.FromResult(requests);
    }

    public Task<List<ServiceReviewResponseDto>> GetReviewsByCustomerAsync(Guid customerId)
    {
        var reviews = _dataStore.ServiceReviews
            .Where(r => r.CustomerId == customerId)
            .Select(r => new ServiceReviewResponseDto
            {
                Id = r.Id,
                CustomerId = r.CustomerId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAtUtc = r.CreatedAtUtc
            }).ToList();

        return Task.FromResult(reviews);
    }

    public Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId)
    {
        var history = _dataStore.SalesInvoices
            .Where(s => s.CustomerId == customerId)
            .Select(s => new PurchaseHistoryDto
            {
                Id = s.Id,
                IssuedAtUtc = s.IssuedAtUtc,
                TotalAmount = s.TotalAmount,
                DiscountAmount = s.DiscountAmount,
                PaidAmount = s.PaidAmount,
                PendingCredit = s.PendingCredit
            }).ToList();

        return Task.FromResult(history);
    }
}