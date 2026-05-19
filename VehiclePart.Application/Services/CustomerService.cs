using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Security;
using VehiclePart.Domain.Entities;

using VehiclePart.Application.Common;

namespace VehiclePart.Application.Services;

public class CustomerService(ICustomerRepository repository, ICustomerHistoryRepository customerHistoryRepository)
    : ICustomerService
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

    public async Task<IReadOnlyList<VehicleMaintenanceReminder>> GetVehicleMaintenanceRemindersAsync(
        Guid vehicleId,
        Guid customerId,
        CancellationToken ct = default)
    {
        var vehicle = await repository.GetVehicleByIdAsync(vehicleId, ct)
            ?? throw new KeyNotFoundException("Vehicle not found.");

        if (vehicle.CustomerId != customerId)
            throw new UnauthorizedAccessException("You do not have access to this vehicle.");

        _ = await repository.GetCustomerAsync(customerId, ct)
            ?? throw new KeyNotFoundException("Customer not found.");

        var reminders = new List<VehicleMaintenanceReminder>();
        var now = DateTime.UtcNow;
        var vehicleAgeYears = Math.Max(0, now.Year - vehicle.Year);

        var partPurchases = (await customerHistoryRepository.GetCustomerPartPurchasesAsync(customerId, ct))
            .Select(line => (line.PartName, line.IssuedAtUtc))
            .ToList();

        static DateTime? LastPurchaseContaining(IEnumerable<(string PartName, DateTime IssuedAtUtc)> purchases, params string[] keywords)
        {
            DateTime? latest = null;
            foreach (var (partName, issuedAt) in purchases)
            {
                if (keywords.Any(keyword => partName.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                    latest = latest is null || issuedAt > latest ? issuedAt : latest;
            }

            return latest;
        }

        void AddIntervalReminder(
            string partLabel,
            string[] keywords,
            int intervalMonths,
            string dueRecommendation,
            string routineRecommendation)
        {
            var lastPurchase = LastPurchaseContaining(partPurchases, keywords);
            if (lastPurchase is null)
            {
                reminders.Add(new VehicleMaintenanceReminder(
                    partLabel,
                    vehicleAgeYears >= 3 ? "Medium" : "Low",
                    routineRecommendation,
                    "No matching purchase on record"));
                return;
            }

            var monthsSince = (now.Year - lastPurchase.Value.Year) * 12 + now.Month - lastPurchase.Value.Month;
            if (monthsSince >= intervalMonths)
            {
                reminders.Add(new VehicleMaintenanceReminder(
                    partLabel,
                    monthsSince >= intervalMonths + 3 ? "High" : "Medium",
                    dueRecommendation,
                    $"Last purchased {lastPurchase.Value:dd MMM yyyy}"));
            }
        }

        AddIntervalReminder(
            "Engine oil & filter",
            ["oil", "filter"],
            6,
            "Oil and filter service is due based on your last purchase date.",
            "Schedule an oil and filter change if you have not serviced this vehicle in the last 6 months.");

        AddIntervalReminder(
            "Brake components",
            ["brake", "pad", "disc"],
            12,
            "Brake inspection or replacement may be due based on your purchase history.",
            "Book a brake inspection if brakes have not been checked in the last 12 months.");

        AddIntervalReminder(
            "Battery",
            ["battery"],
            24,
            "Battery replacement may be due based on your purchase history.",
            "Consider a battery test if this vehicle has not had a battery service in 2 years.");

        if (vehicleAgeYears >= 5)
        {
            var beltPurchase = LastPurchaseContaining(partPurchases, "belt", "timing");
            if (beltPurchase is null || now - beltPurchase.Value > TimeSpan.FromDays(365 * 5))
            {
                reminders.Add(new VehicleMaintenanceReminder(
                    "Timing / drive belt",
                    vehicleAgeYears >= 8 ? "High" : "Medium",
                    "Older vehicles should have belts inspected on the manufacturer schedule.",
                    beltPurchase is null
                        ? "No belt-related purchase on record"
                        : $"Last belt-related purchase {beltPurchase.Value:dd MMM yyyy}"));
            }
        }

        if (vehicleAgeYears >= 4 && !reminders.Any(r => r.Priority == "High"))
        {
            reminders.Add(new VehicleMaintenanceReminder(
                "General inspection",
                "Low",
                $"This {vehicle.Make} {vehicle.Model} ({vehicle.Year}) is due for a routine workshop inspection.",
                "Based on vehicle registration year"));
        }

        if (reminders.Count == 0)
        {
            reminders.Add(new VehicleMaintenanceReminder(
                "Routine service",
                "Low",
                "No overdue items were found from your purchase records. Book a service appointment for a full workshop check.",
                "Purchase history review"));
        }

        return reminders
            .OrderByDescending(r => r.Priority switch
            {
                "High" => 3,
                "Medium" => 2,
                _ => 1,
            })
            .ThenBy(r => r.PartName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public Task<IReadOnlyList<DateTime>> GetBookedAppointmentTimesForDayAsync(
        int year,
        int month,
        int day,
        CancellationToken ct = default)
    {
        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
            throw new ArgumentException("Invalid date.");

        return repository.GetAppointmentTimesForNepalLocalDayAsync(year, month, day, ct);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetAppointmentsAsync(Guid customerId, CancellationToken ct = default)
    {
        var list = await repository.GetAppointmentsByCustomerIdAsync(customerId, ct);
        return list.Select(a => new AppointmentDto(
            a.Id,
            a.AppointmentDate,
            a.ServiceType,
            a.Status,
            a.Notes,
            a.VehicleId)).ToList();
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetReviewableAppointmentsAsync(Guid customerId, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");

        var list = await repository.GetReviewableAppointmentsAsync(customerId, ct);
        return list.Select(a => new AppointmentDto(
            a.Id,
            a.AppointmentDate,
            a.ServiceType,
            a.Status,
            a.Notes,
            a.VehicleId)).ToList();
    }

    public async Task BookAppointmentAsync(Guid customerId, BookAppointmentDto dto, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");
        var appointmentDateUtc = NormalizeAppointmentUtc(dto.AppointmentDate);

        if (string.IsNullOrWhiteSpace(dto.ServiceType))
            throw new ArgumentException("Service type is required.", nameof(dto));

        if (appointmentDateUtc < DateTime.UtcNow)
            throw new ArgumentException("Appointment date cannot be in the past.");

        if (dto.VehicleId == Guid.Empty)
            throw new ArgumentException("Vehicle is required.", nameof(dto));

        var vehicle = await repository.GetVehicleByIdAsync(dto.VehicleId, ct);
        if (vehicle is null || vehicle.CustomerId != customerId)
            throw new ArgumentException("Selected vehicle was not found on your account.", nameof(dto));

        var appointment = new Appointment
        {
            CustomerId = customerId,
            VehicleId = dto.VehicleId,
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
            throw new InvalidOperationException("You can only review a service after staff marks it as completed.");

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

    private static bool IsServiceTaken(Appointment appointment) =>
        AppointmentStatuses.CanBeReviewed(appointment.Status);

    public async Task<List<PurchaseHistoryDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken ct = default)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Invalid customer ID.");

        var invoices = await repository.GetPurchaseHistoryAsync(customerId, ct);
        return invoices.Select(i => new PurchaseHistoryDto(
            i.Id, i.TotalAmount, i.DiscountAmount, i.PaidAmount, i.PendingCredit, i.IssuedAtUtc
        )).ToList();
    }

    private static DateTime NormalizeAppointmentUtc(DateTime value)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

        return new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, DateTimeKind.Utc);
    }
}
