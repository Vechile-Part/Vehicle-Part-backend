using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class CustomerHistoryRepository(AppDbContext dbContext) : ICustomerHistoryRepository
{
    public async Task<CustomerHistoryDto?> GetCustomerHistoryAsync(
        Guid customerId,
        bool includeAppointmentsAndReviews = true,
        CancellationToken cancellationToken = default)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return null;

        var vehicles = await dbContext.Vehicles
            .AsNoTracking()
            .Where(v => v.CustomerId == customerId)
            .OrderByDescending(v => v.Year)
            .ThenBy(v => v.VehicleNumber)
            .ToListAsync(cancellationToken);

        var invoices = await dbContext.SalesInvoices
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.IssuedAtUtc)
            .ToListAsync(cancellationToken);

        var invoiceIds = invoices.Select(i => i.Id).ToList();
        var invoiceLines = invoiceIds.Count == 0
            ? []
            : await (
                from item in dbContext.SalesInvoiceItems.AsNoTracking()
                join part in dbContext.Parts.AsNoTracking() on item.PartId equals part.Id into partJoin
                from part in partJoin.DefaultIfEmpty()
                where invoiceIds.Contains(item.SalesInvoiceId)
                select new
                {
                    item.SalesInvoiceId,
                    PartName = part != null ? part.Name : "Part",
                    item.Quantity,
                    item.UnitPrice,
                }).ToListAsync(cancellationToken);

        var linesByInvoice = invoiceLines
            .GroupBy(line => line.SalesInvoiceId)
            .ToDictionary(group => group.Key, group => group.ToList());

        List<AppointmentDto> appointmentDtos = [];
        List<ServiceReviewHistoryDto> reviewDtos = [];

        if (includeAppointmentsAndReviews)
        {
            var appointments = await dbContext.Appointments
                .AsNoTracking()
                .Where(a => a.CustomerId == customerId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync(cancellationToken);

            var reviews = await dbContext.ServiceReviews
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.Id)
                .ToListAsync(cancellationToken);

            appointmentDtos = appointments
                .Select(a => new AppointmentDto(
                    a.Id,
                    a.AppointmentDate,
                    a.ServiceType,
                    a.Status,
                    a.Notes))
                .ToList();

            reviewDtos = reviews
                .Select(r => new ServiceReviewHistoryDto
                {
                    Id = r.Id,
                    ServiceId = r.ServiceId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                })
                .ToList();
        }

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
                Year = v.Year,
            }).ToList(),

            Invoices = invoices.Select(i => new CustomerInvoiceDto
            {
                InvoiceId = i.Id,
                IssuedAtUtc = i.IssuedAtUtc,
                TotalAmount = i.TotalAmount,
                DiscountAmount = i.DiscountAmount,
                PaidAmount = i.PaidAmount,
                PendingCredit = i.PendingCredit,
                Items = linesByInvoice.TryGetValue(i.Id, out var lines)
                    ? lines.Select(line => new CustomerInvoiceLineDto
                    {
                        PartName = line.PartName,
                        Quantity = line.Quantity,
                        UnitPrice = line.UnitPrice,
                        LineTotal = line.Quantity * line.UnitPrice,
                    }).ToList()
                    : [],
            }).ToList(),

            Appointments = appointmentDtos,
            ServiceReviews = reviewDtos,
        };
    }

    public async Task<IReadOnlyList<CustomerPartPurchaseLineDto>> GetCustomerPartPurchasesAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await (
            from item in dbContext.SalesInvoiceItems.AsNoTracking()
            join invoice in dbContext.SalesInvoices.AsNoTracking() on item.SalesInvoiceId equals invoice.Id
            join part in dbContext.Parts.AsNoTracking() on item.PartId equals part.Id into partJoin
            from part in partJoin.DefaultIfEmpty()
            where invoice.CustomerId == customerId
            orderby invoice.IssuedAtUtc descending
            select new CustomerPartPurchaseLineDto(
                part != null ? part.Name : "Part",
                invoice.IssuedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
