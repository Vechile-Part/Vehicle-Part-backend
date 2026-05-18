using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Security;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Services;

public class CustomerService(ICustomerRepository repository) : ICustomerService
{
    public async Task<Guid> SelfRegisterAsync(CustomerSelfRegistrationDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password is required.", nameof(dto));

        if (dto.Password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters.", nameof(dto));

        if (await repository.GetCustomerByEmailAsync(dto.Email, ct) is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var customer = await repository.AddCustomerAsync(new Customer
        {
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email.Trim(),
            PasswordHash = CustomerPasswordHasher.HashPassword(dto.Password)
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
        return customer is null ? null : new CustomerProfileDto(customer.Id, customer.FullName, customer.Phone, customer.Email, customer.ProfilePictureUrl);
    }

    public async Task UpdateProfileAsync(Guid customerId, CustomerProfileDto dto, CancellationToken ct = default)
    {
        await repository.UpdateCustomerAsync(new Customer
        {
            Id = customerId,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            ProfilePictureUrl = dto.ProfilePictureUrl
        }, ct);
    }

    public async Task<IReadOnlyList<VehicleDto>> GetCustomerVehiclesAsync(Guid customerId, CancellationToken ct = default)
    {
        var vehicles = await repository.GetVehiclesByCustomerIdAsync(customerId, ct);
        return vehicles.Select(v => new VehicleDto(v.Id, v.VehicleNumber, v.Make, v.Model, v.Year)).ToList();
    }

    public async Task UpdateVehicleAsync(Guid customerId, VehicleDto dto, CancellationToken ct = default)
    {
        var existing = await repository.GetVehicleByIdAsync(dto.Id, ct)
            ?? throw new KeyNotFoundException("Vehicle not found.");

        if (existing.CustomerId != customerId)
            throw new UnauthorizedAccessException("You cannot modify this vehicle.");

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

    public async Task DeleteVehicleAsync(Guid customerId, Guid vehicleId, CancellationToken ct = default)
    {
        var existing = await repository.GetVehicleByIdAsync(vehicleId, ct)
            ?? throw new KeyNotFoundException("Vehicle not found.");

        if (existing.CustomerId != customerId)
            throw new UnauthorizedAccessException("You cannot delete this vehicle.");

        await repository.DeleteVehicleAsync(customerId, vehicleId, ct);
    }

    public async Task ChangePasswordAsync(Guid customerId, ChangeCustomerPasswordDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
            throw new ArgumentException("New password must be at least 8 characters.", nameof(dto));

        var customer = await repository.GetCustomerAsync(customerId, ct)
            ?? throw new KeyNotFoundException("Customer not found.");

        if (!CustomerPasswordHasher.VerifyPassword(dto.CurrentPassword, customer.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        await repository.SetCustomerPasswordHashAsync(
            customerId,
            CustomerPasswordHasher.HashPassword(dto.NewPassword),
            ct);
    }

    public async Task<IReadOnlyList<VehicleHealthInsight>> GetVehicleHealthAIAsync(Guid vehicleId, Guid customerId, CancellationToken ct = default)
    {
        var vehicle = await repository.GetVehicleByIdAsync(vehicleId, ct)
            ?? throw new KeyNotFoundException("Vehicle not found.");

        if (vehicle.CustomerId != customerId)
            throw new UnauthorizedAccessException("You do not have access to this vehicle.");

        var insights = new List<VehicleHealthInsight>
        {
            new("Brake Pads", 0.75, "High wear detected. Schedule replacement within 15 days.", "15 days"),
            new("Timing Belt", 0.30, "Condition normal. Inspect in 6 months.", "180 days"),
            new("Air Filter", 0.90, "Critical blockage. Replace immediately.", "2 days")
        };

        return await Task.FromResult<IReadOnlyList<VehicleHealthInsight>>(insights);
    }

    public Task<IReadOnlyList<DateTime>> GetBookedAppointmentTimesForDayAsync(
        int year,
        int month,
        int day,
        CancellationToken ct = default)
    {
        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
            throw new ArgumentException("Invalid date.");

        var dateUtc = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        return repository.GetAppointmentTimesOnUtcDateAsync(dateUtc, ct);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetAppointmentsAsync(Guid customerId, CancellationToken ct = default)
    {
        var list = await repository.GetAppointmentsByCustomerIdAsync(customerId, ct);
        return list.Select(a => new AppointmentDto(a.Id, a.AppointmentDate, a.ServiceType, a.Status, a.Notes)).ToList();
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetReviewableAppointmentsAsync(Guid customerId, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");

        var list = await repository.GetReviewableAppointmentsAsync(customerId, ct);
        return list.Select(a => new AppointmentDto(a.Id, a.AppointmentDate, a.ServiceType, a.Status, a.Notes)).ToList();
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

        if (string.IsNullOrWhiteSpace(dto.ServiceType))
            throw new ArgumentException("Service type is required.", nameof(dto));

        if (appointmentDateUtc < DateTime.UtcNow)
            throw new ArgumentException("Appointment date cannot be in the past.");

        var appointment = new Appointment
        {
            CustomerId = customerId,
            AppointmentDate = appointmentDateUtc,
            ServiceType = dto.ServiceType.Trim(),
            Notes = dto.Notes
        };

        if (!await repository.TryAddAppointmentAsync(appointment, ct))
            throw new InvalidOperationException("This time slot is already booked. Please choose another time.");
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
        if (dto.ServiceId == Guid.Empty)
            throw new ArgumentException("Service is required.");
        if (dto.Rating < 1 || dto.Rating > 5)
            throw new ArgumentOutOfRangeException(nameof(dto.Rating), "Rating must be between 1 and 5.");

        var appointment = await repository.GetAppointmentForCustomerAsync(customerId, dto.ServiceId, ct)
            ?? throw new InvalidOperationException("Service appointment not found.");

        if (!IsServiceTaken(appointment))
            throw new InvalidOperationException("You can only review a service after it has taken place.");

        if (await repository.HasServiceReviewAsync(customerId, dto.ServiceId, ct))
            throw new InvalidOperationException("You have already submitted a review for this service.");

        await repository.AddServiceReviewAsync(new ServiceReview
        {
            CustomerId = customerId,
            ServiceId = dto.ServiceId,
            Rating = dto.Rating,
            Comment = dto.Comment
        }, ct);
    }

    private static bool IsServiceTaken(Appointment appointment)
    {
        if (string.Equals(appointment.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return false;

        if (string.Equals(appointment.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            return true;

        return appointment.AppointmentDate <= DateTime.UtcNow;
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
}
