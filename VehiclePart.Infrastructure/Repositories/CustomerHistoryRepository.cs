using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class CustomerHistoryRepository(AppDbContext dbContext) : ICustomerHistoryRepository
{
    public async Task<CustomerHistoryDto?> GetCustomerHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return null;

        var vehicles = await dbContext.Vehicles
            .Where(v => v.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        var invoices = await dbContext.SalesInvoices
            .Where(i => i.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        var appointments = await dbContext.Appointments
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);

        var reviews = await dbContext.ServiceReviews
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.Id)
            .ToListAsync(cancellationToken);

        return new CustomerHistoryDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            ProfilePictureUrl = customer.ProfilePictureUrl,

            Vehicles = vehicles.Select(v => new CustomerVehicleDto
            {
                VehicleId = v.Id,
                VehicleNumber = v.VehicleNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year
            }).ToList(),

            Invoices = invoices.Select(i => new CustomerInvoiceDto
            {
                InvoiceId = i.Id,
                IssuedAtUtc = i.IssuedAtUtc,
                TotalAmount = i.TotalAmount,
                DiscountAmount = i.DiscountAmount,
                PaidAmount = i.PaidAmount,
                PendingCredit = i.PendingCredit
            }).ToList(),

            Appointments = appointments.Select(a => new AppointmentDto(
                a.Id,
                a.AppointmentDate,
                a.ServiceType,
                a.Status,
                a.Notes)).ToList(),

            ServiceReviews = reviews.Select(r => new ServiceReviewHistoryDto
            {
                Id = r.Id,
                ServiceId = r.ServiceId,
                Rating = r.Rating,
                Comment = r.Comment
            }).ToList()
        };
    }
}