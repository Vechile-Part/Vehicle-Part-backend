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
            .AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new CustomerHistoryDto
            {
                CustomerId = c.Id,
                CustomerName = c.FullName,
                Phone = c.Phone,
                Email = c.Email
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
            return null;

        customer.Vehicles = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.CustomerId == customerId)
            .Select(v => new CustomerVehicleDto
            {
                VehicleId = v.Id,
                VehicleNumber = v.VehicleNumber,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year
            })
            .ToListAsync(cancellationToken);

        customer.Invoices = await dbContext.SalesInvoices
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId)
            .Select(i => new CustomerInvoiceDto
            {
                InvoiceId = i.Id,
                IssuedAtUtc = i.IssuedAtUtc,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                PendingCredit = i.PendingCredit
            })
            .ToListAsync(cancellationToken);

        return customer;
    }
}