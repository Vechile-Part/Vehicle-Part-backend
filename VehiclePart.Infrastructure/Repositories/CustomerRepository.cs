using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        dbContext.Vehicles.Add(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
    }

    public Task<Customer?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        return dbContext.Customers
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalized.ToLower(), cancellationToken);
    }

    public async Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id, cancellationToken);
        if (existing is null) return;

        existing.FullName = customer.FullName;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.ProfilePictureUrl = customer.ProfilePictureUrl;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Vehicle>> GetVehiclesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles.Where(x => x.CustomerId == customerId).ToListAsync(cancellationToken);
    }

    public async Task<Vehicle?> GetVehicleByIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId, cancellationToken);
    }

    public async Task UpdateVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicle.Id, cancellationToken);
        if (existing is null) return;

        existing.VehicleNumber = vehicle.VehicleNumber;
        existing.Make = vehicle.Make;
        existing.Model = vehicle.Model;
        existing.Year = vehicle.Year;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddPartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default)
    {
        dbContext.PartRequests.Add(partRequest);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddServiceReviewAsync(ServiceReview review, CancellationToken cancellationToken = default)
    {
        dbContext.ServiceReviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<SalesInvoice>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesInvoices
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddCustomerPasswordSetupTokenAsync(CustomerPasswordSetupToken token, CancellationToken cancellationToken = default)
    {
        dbContext.CustomerPasswordSetupTokens.Add(token);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CustomerPasswordSetupToken?> GetActivePasswordSetupTokenByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await dbContext.CustomerPasswordSetupTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.TokenHash == tokenHash && t.UsedAtUtc == null && t.ExpiresAtUtc > now,
                cancellationToken);
    }

    public async Task MarkPasswordSetupTokenUsedAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        var row = await dbContext.CustomerPasswordSetupTokens.FirstOrDefaultAsync(t => t.Id == tokenId, cancellationToken);
        if (row is null) return;
        row.UsedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task InvalidateUnusedPasswordSetupTokensForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var rows = await dbContext.CustomerPasswordSetupTokens
            .Where(t => t.CustomerId == customerId && t.UsedAtUtc == null)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var r in rows)
            r.UsedAtUtc = now;
        if (rows.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetCustomerPasswordHashAsync(Guid customerId, string passwordHash, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
        if (existing is null) return;
        existing.PasswordHash = passwordHash;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}