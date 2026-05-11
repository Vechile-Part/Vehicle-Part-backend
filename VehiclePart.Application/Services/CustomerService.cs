using System.Security.Cryptography;
using System.Text;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Services;

public class CustomerService(ICustomerRepository repository) : ICustomerService
{
    public async Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken ct = default)
    {
        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password)
        }, ct);
        return customer.Id;
    }

    public Task AddVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken ct = default)
    {
        return repository.AddVehicleAsync(new Vehicle
        {
            CustomerId = customerId,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, ct);
    }

    public async Task<CustomerProfileDto?> GetProfileAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await repository.GetCustomerAsync(customerId, ct);
        return customer is null ? null : new CustomerProfileDto(customer.Id, customer.FullName, customer.Phone, customer.Email);
    }

    public async Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken ct = default)
    {
        await repository.UpdateCustomerAsync(new Customer
        {
            Id = customerId,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone
        }, ct);
    }

    public async Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken ct = default)
    {
        var vehicles = await repository.GetVehiclesByCustomerIdAsync(customerId, ct);
        return vehicles.Select(v => new VehicleDto(v.Id, v.VehicleNumber, v.Make, v.Model, v.Year)).ToList();
    }

    public async Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken ct = default)
    {
        await repository.UpdateVehicleAsync(new Vehicle
        {
            Id = dto.Id,
            CustomerId = customerId,
            VehicleNumber = dto.VehicleNumber,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year
        }, ct);
    }

    public async Task<IReadOnlyList<VehicleHealthInsight>> GetVehicleHealthAIAsync(Guid vehicleId, CancellationToken ct = default)
    {
        if (await repository.GetVehicleByIdAsync(vehicleId, ct) is null)
            throw new KeyNotFoundException("Vehicle not found.");

        var insights = new List<VehicleHealthInsight>
        {
            new("Brake Pads", 0.75, "High wear detected. Schedule replacement within 15 days.", "15 days"),
            new("Timing Belt", 0.30, "Condition normal. Inspect in 6 months.", "180 days"),
            new("Air Filter", 0.90, "Critical blockage. Replace immediately.", "2 days")
        };

        return await Task.FromResult<IReadOnlyList<VehicleHealthInsight>>(insights);
    }

    public async Task BookAppointmentAsync(Guid customerId, BookAppointmentDto dto, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");
        var appointmentDateUtc = dto.AppointmentDate.Kind switch
        {
            DateTimeKind.Utc => dto.AppointmentDate,
            DateTimeKind.Local => dto.AppointmentDate.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dto.AppointmentDate, DateTimeKind.Utc)
        };

        if (appointmentDateUtc < DateTime.UtcNow)
            throw new ArgumentException("Appointment date cannot be in the past.");

        await repository.AddAppointmentAsync(new Appointment
        {
            CustomerId = customerId,
            AppointmentDate = appointmentDateUtc,
            ServiceType = dto.ServiceType,
            Notes = dto.Notes
        }, ct);
    }

    public async Task RequestPartAsync(Guid customerId, PartRequestDto dto, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");
        if (string.IsNullOrWhiteSpace(dto.PartName))
            throw new ArgumentNullException(nameof(dto.PartName), "Part name is required.");

        await repository.AddPartRequestAsync(new PartRequest
        {
            CustomerId = customerId,
            PartName = dto.PartName,
            Description = dto.Description
        }, ct);
    }  

    public async Task ReviewServiceAsync(Guid customerId, ServiceReviewDto dto, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new ArgumentOutOfRangeException(nameof(dto.Rating), "Rating must be between 1 and 5.");

        await repository.AddServiceReviewAsync(new ServiceReview
        {
            CustomerId = customerId,
            ServiceId = dto.ServiceId,
            Rating = dto.Rating,
            Comment = dto.Comment
        }, ct);
    }

    public async Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");

        var invoices = await repository.GetPurchaseHistoryAsync(customerId, ct);
        return invoices.Select(i => new PurchaseHistoryDto(
            i.Id, i.TotalAmount, i.DiscountAmount, i.PaidAmount, i.PendingCredit, i.IssuedAtUtc
        )).ToList();
    }
    
    public async Task<List<AppointmentDto>> GetAppointmentsAsync(Guid customerId, CancellationToken ct = default)
    {
        var appointments = await repository.GetAppointmentsByCustomerIdAsync(customerId, ct);
        return appointments.Select(a => new AppointmentDto(a.Id, a.AppointmentDate, a.ServiceType, a.Notes)).ToList();
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