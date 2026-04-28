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

    public async Task<IReadOnlyList<VehicleHealthInsight>> GetVehicleHealthAIAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        if (await repository.GetVehicleByIdAsync(vehicleId, cancellationToken) is null)
            throw new KeyNotFoundException("Vehicle not found.");

        var insights = new List<VehicleHealthInsight>
        {
            new("Brake Pads", 0.75, "High wear detected. Schedule replacement within 15 days.", "15 days"),
            new("Timing Belt", 0.30, "Condition normal. Inspect in 6 months.", "180 days"),
            new("Air Filter", 0.90, "Critical blockage. Replace immediately.", "2 days")
        };

        return await Task.FromResult<IReadOnlyList<VehicleHealthInsight>>(insights);
    }

    public async Task<Guid> BookAppointmentAsync(Guid customerId, AppointmentDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var appointment = await repository.AddAppointmentAsync(new Appointment
        {
            CustomerId = customerId,
            AppointmentAtUtc = dto.AppointmentAtUtc,
            Notes = dto.Notes
        }, cancellationToken);

        return appointment.Id;
    }

    public async Task<Guid> RequestPartAsync(Guid customerId, PartRequestDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var request = await repository.AddPartRequestAsync(new PartRequest
        {
            CustomerId = customerId,
            PartName = dto.PartName,
            Notes = dto.Notes,
            RequestedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return request.Id;
    }

    public async Task<Guid> SubmitServiceReviewAsync(Guid customerId, ServiceReviewDto dto, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        if (dto.Rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(dto.Rating), "Rating must be between 1 and 5.");

        var review = await repository.AddServiceReviewAsync(new ServiceReview
        {
            CustomerId = customerId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAtUtc = DateTime.UtcNow
        }, cancellationToken);

        return review.Id;
    }

    public async Task<IReadOnlyList<AppointmentResponseDto>> GetAppointmentsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var items = await repository.GetAppointmentsByCustomerIdAsync(customerId, cancellationToken);
        return items.Select(a => new AppointmentResponseDto(a.Id, a.CustomerId, a.AppointmentAtUtc, a.Notes)).ToList();
    }

    public async Task<IReadOnlyList<PartRequestResponseDto>> GetPartRequestsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var items = await repository.GetPartRequestsByCustomerIdAsync(customerId, cancellationToken);
        return items.Select(p => new PartRequestResponseDto(
            p.Id,
            p.CustomerId,
            p.PartName,
            p.Notes,
            DateTime.UtcNow)).ToList();
    }

    public async Task<IReadOnlyList<ServiceReviewResponseDto>> GetReviewsByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var items = await repository.GetServiceReviewsByCustomerIdAsync(customerId, cancellationToken);
        return items.Select(r => new ServiceReviewResponseDto(
            r.Id,
            r.CustomerId,
            r.Rating,
            r.Comment,
            DateTime.UtcNow)).ToList();
    }

    public async Task<IReadOnlyList<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var invoices = await repository.GetSalesInvoicesByCustomerIdAsync(customerId, cancellationToken);
        return invoices.Select(s => new PurchaseHistoryDto(
            s.Id,
            s.IssuedAtUtc,
            s.TotalAmount,
            s.DiscountAmount,
            s.PaidAmount,
            s.PendingCredit)).ToList();
    }

    private async Task EnsureCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        if (await repository.GetCustomerAsync(customerId, cancellationToken) is null)
            throw new KeyNotFoundException("Customer not found.");
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
