using Microsoft.EntityFrameworkCore;
using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer> AddCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
    {
        dbContext.Vehicles.Add(vehicle);
        await dbContext.SaveChangesAsync(cancellationToken);
        return vehicle;
    }

    public async Task<Customer?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customerId, cancellationToken);
    }

    public async Task UpdateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id, cancellationToken);
        if (existing is null) return;

        existing.FullName = customer.FullName;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Vehicle>> GetVehiclesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
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

    public async Task<Appointment> AddAppointmentAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return appointment;
    }

    public async Task<PartRequest> AddPartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default)
    {
        dbContext.PartRequests.Add(partRequest);
        await dbContext.SaveChangesAsync(cancellationToken);
        return partRequest;
    }

    public async Task<ServiceReview> AddServiceReviewAsync(ServiceReview review, CancellationToken cancellationToken = default)
    {
        dbContext.ServiceReviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);
        return review;
    }

    public async Task<IReadOnlyList<Appointment>> GetAppointmentsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Appointments
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.AppointmentAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PartRequest>> GetPartRequestsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.PartRequests
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceReview>> GetServiceReviewsByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ServiceReviews
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesInvoices
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.IssuedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesInvoices.ToListAsync(cancellationToken);
    }

    public async Task<SalesInvoice?> GetSalesInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SalesInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Customers.ToListAsync(cancellationToken);
    }
}
