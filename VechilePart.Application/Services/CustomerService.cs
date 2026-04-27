using System.Security.Cryptography;
using System.Text;
using VechilePart.Application.DTOs;
using VechilePart.Application.Interfaces;
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
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password)
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

    public async Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetCustomerAsync(customerId, cancellationToken);
        return customer is null ? null : new CustomerProfileDto(customer.Id, customer.FullName, customer.Phone, customer.Email);
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

    private static string HashPassword(string password)
    {
        const int iterations = 100_000;
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            32);

        return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}

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
