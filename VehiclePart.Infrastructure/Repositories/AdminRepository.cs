using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;

using VehiclePart.Infrastructure.Data;



namespace VehiclePart.Infrastructure.Repositories;



public class AdminRepository(AppDbContext dbContext) : IAdminRepository

{

    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) 

        => await dbContext.SalesInvoices.ToListAsync(cancellationToken);



    public async Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default) 

        => await dbContext.PurchaseInvoices.ToListAsync(cancellationToken);



    public async Task<IReadOnlyList<Part>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default)

        => await dbContext.Parts.Where(x => x.QuantityInStock < threshold).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<(Part Part, Guid EffectiveVendorId, string? VendorName)>> GetAllPartsWithVendorNamesAsync(
        CancellationToken cancellationToken = default)
    {

        var rows = await (

            from part in dbContext.Parts.AsNoTracking()

            join vendor in dbContext.Vendors.AsNoTracking() on part.VendorId equals vendor.Id into vendorJoin

            from vendor in vendorJoin.DefaultIfEmpty()

            orderby part.Name

            select new

            {

                Part = part,

                DirectVendorName = vendor != null ? vendor.Name : null

            }

        ).ToListAsync(cancellationToken);



        if (rows.Count == 0)

            return Array.Empty<(Part, Guid, string?)>();



        var vendorNames = await dbContext.Vendors.AsNoTracking()

            .ToDictionaryAsync(vendor => vendor.Id, vendor => vendor.Name, cancellationToken);



        var partIds = rows.Select(row => row.Part.Id).ToList();



        var purchaseRows = await (

            from item in dbContext.PurchaseInvoiceItems.AsNoTracking()

            join invoice in dbContext.PurchaseInvoices.AsNoTracking() on item.PurchaseInvoiceId equals invoice.Id

            where partIds.Contains(item.PartId) && invoice.VendorId != Guid.Empty

            orderby invoice.IssuedAtUtc descending

            select new { item.PartId, invoice.VendorId }

        ).ToListAsync(cancellationToken);



        var latestVendorByPart = new Dictionary<Guid, Guid>();

        foreach (var row in purchaseRows)

        {

            if (!latestVendorByPart.ContainsKey(row.PartId))

                latestVendorByPart[row.PartId] = row.VendorId;

        }



        return rows
            .Select(row =>
            {
                var vendorId = row.Part.VendorId;
                string? name = row.DirectVendorName;

                if (vendorId != Guid.Empty && !vendorNames.ContainsKey(vendorId))
                {
                    vendorId = Guid.Empty;
                    name = null;
                }

                if (vendorId == Guid.Empty && latestVendorByPart.TryGetValue(row.Part.Id, out var fromPurchase))
                    vendorId = fromPurchase;

                if (string.IsNullOrWhiteSpace(name) && vendorId != Guid.Empty)
                    vendorNames.TryGetValue(vendorId, out name);

                return (row.Part, vendorId, name);
            })
            .ToList();

    }

    public async Task<(IReadOnlyList<(Part Part, Guid EffectiveVendorId, string? VendorName)> Items, int TotalCount)> GetPagedPartsWithVendorNamesAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Parts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(s) || x.PartNumber.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rows = await (
            from part in query
            join vendor in dbContext.Vendors.AsNoTracking() on part.VendorId equals vendor.Id into vendorJoin
            from vendor in vendorJoin.DefaultIfEmpty()
            orderby part.Name
            select new
            {
                Part = part,
                DirectVendorName = vendor != null ? vendor.Name : null
            }
        )
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

        if (rows.Count == 0)
            return (Array.Empty<(Part, Guid, string?)>(), totalCount);

        var vendorNames = await dbContext.Vendors.AsNoTracking()
            .ToDictionaryAsync(vendor => vendor.Id, vendor => vendor.Name, cancellationToken);

        var partIds = rows.Select(row => row.Part.Id).ToList();

        var purchaseRows = await (
            from item in dbContext.PurchaseInvoiceItems.AsNoTracking()
            join invoice in dbContext.PurchaseInvoices.AsNoTracking() on item.PurchaseInvoiceId equals invoice.Id
            where partIds.Contains(item.PartId) && invoice.VendorId != Guid.Empty
            orderby invoice.IssuedAtUtc descending
            select new { item.PartId, invoice.VendorId }
        ).ToListAsync(cancellationToken);

        var latestVendorByPart = new Dictionary<Guid, Guid>();
        foreach (var row in purchaseRows)
        {
            if (!latestVendorByPart.ContainsKey(row.PartId))
                latestVendorByPart[row.PartId] = row.VendorId;
        }

        var items = rows
            .Select(row =>
            {
                var vendorId = row.Part.VendorId;
                string? name = row.DirectVendorName;

                if (vendorId != Guid.Empty && !vendorNames.ContainsKey(vendorId))
                {
                    vendorId = Guid.Empty;
                    name = null;
                }

                if (vendorId == Guid.Empty && latestVendorByPart.TryGetValue(row.Part.Id, out var fromPurchase))
                    vendorId = fromPurchase;

                if (string.IsNullOrWhiteSpace(name) && vendorId != Guid.Empty)
                    vendorNames.TryGetValue(vendorId, out name);

                return (row.Part, vendorId, name);
            })
            .ToList();

        return (items, totalCount);
    }



    public async Task<IReadOnlyList<OverdueCreditInvoiceDto>> GetOverdueCreditInvoicesAsync(

        int minimumAgeMonths,

        CancellationToken cancellationToken = default)

    {

        var cutoff = DateTime.UtcNow.AddMonths(-minimumAgeMonths);



        return await (

            from invoice in dbContext.SalesInvoices.AsNoTracking()

            join customer in dbContext.Customers.AsNoTracking() on invoice.CustomerId equals customer.Id

            where invoice.PendingCredit > 0

                  && invoice.IssuedAtUtc <= cutoff

            orderby invoice.IssuedAtUtc

            select new OverdueCreditInvoiceDto(

                invoice.Id,

                customer.Id,

                customer.FullName,

                customer.Email,

                invoice.PendingCredit,

                invoice.IssuedAtUtc,

                invoice.LastCreditReminderSentUtc)

        ).ToListAsync(cancellationToken);

    }



    public async Task AddUserAsync(User user, CancellationToken cancellationToken = default)

    {

        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync(cancellationToken);

    }



    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)

        => await dbContext.Users.FindAsync(id, cancellationToken);



    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)

    {

        dbContext.Users.Update(user);

        await dbContext.SaveChangesAsync(cancellationToken);

    }



    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)

        => await dbContext.Users.AsNoTracking().ToListAsync(cancellationToken);



    public async Task<IReadOnlyList<User>> GetStaffUsersAsync(CancellationToken cancellationToken = default)

        => await dbContext.Users

            .AsNoTracking()

            .Where(u => u.Role != RoleType.Customer)

            .OrderBy(u => u.FullName)

            .ThenBy(u => u.Email)

            .ToListAsync(cancellationToken);



    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)

    {

        var user = await dbContext.Users.FindAsync(id, cancellationToken);

        if (user is not null)

        {

            dbContext.Users.Remove(user);

            await dbContext.SaveChangesAsync(cancellationToken);

        }

    }



    public async Task AddPartAsync(Part part, CancellationToken cancellationToken = default)

    {

        dbContext.Parts.Add(part);

        await dbContext.SaveChangesAsync(cancellationToken);

    }



    public async Task<Part?> GetPartByIdAsync(Guid id, CancellationToken cancellationToken = default)

        => await dbContext.Parts.FindAsync(id, cancellationToken);



    public async Task UpdatePartAsync(Part part, CancellationToken cancellationToken = default)

    {

        dbContext.Parts.Update(part);

        await dbContext.SaveChangesAsync(cancellationToken);

    }



    public async Task<bool> IsPartReferencedAsync(Guid partId, CancellationToken cancellationToken = default)
    {
        var onPurchase = await dbContext.PurchaseInvoiceItems
            .AsNoTracking()
            .AnyAsync(item => item.PartId == partId, cancellationToken);
        if (onPurchase)
            return true;

        return await dbContext.SalesInvoiceItems
            .AsNoTracking()
            .AnyAsync(item => item.PartId == partId, cancellationToken);
    }

    public async Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var part = await dbContext.Parts.FindAsync(id, cancellationToken);
        if (part is not null)
        {
            part.IsDeleted = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }



    public async Task AddPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default)

    {

        dbContext.PurchaseInvoices.Add(invoice);

        await dbContext.SaveChangesAsync(cancellationToken);

    }



    public async Task<IReadOnlyList<PartRequestAdminDto>> GetPartRequestsAsync(CancellationToken cancellationToken = default)

    {

        return await (

            from request in dbContext.PartRequests.AsNoTracking()

            join customer in dbContext.Customers.AsNoTracking() on request.CustomerId equals customer.Id

            orderby request.Status == "Pending" ? 0 : 1, request.PartName

            select new PartRequestAdminDto(

                request.Id,

                customer.Id,

                customer.FullName,

                customer.Email,

                customer.Phone,

                request.PartName,

                request.Description,

                request.Status)

        ).ToListAsync(cancellationToken);

    }



    public Task<PartRequest?> GetPartRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)

        => dbContext.PartRequests.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);



    public async Task UpdatePartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default)

    {

        dbContext.PartRequests.Update(partRequest);

        await dbContext.SaveChangesAsync(cancellationToken);

    }

}


