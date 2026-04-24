using Microsoft.EntityFrameworkCore;
using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public class AdminRepository(AppDbContext dbContext) : IAdminRepository
{
    public async Task<User> AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<Part> UpsertPartAsync(Part part, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Parts.FirstOrDefaultAsync(x => x.Id == part.Id, cancellationToken);
        if (existing is null)
        {
            dbContext.Parts.Add(part);
            await dbContext.SaveChangesAsync(cancellationToken);
            return part;
        }

        existing.Name = part.Name;
        existing.PartNumber = part.PartNumber;
        existing.UnitPrice = part.UnitPrice;
        existing.QuantityInStock = part.QuantityInStock;
        existing.VendorId = part.VendorId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeletePartAsync(Guid partId, CancellationToken cancellationToken = default)
    {
        var part = await dbContext.Parts.FirstOrDefaultAsync(x => x.Id == partId, cancellationToken);
        if (part != null)
        {
            dbContext.Parts.Remove(part);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<PurchaseInvoice> AddPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.PurchaseInvoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);
        return invoice;
    }

    public async Task<Vendor> UpsertVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Vendors.FirstOrDefaultAsync(x => x.Id == vendor.Id, cancellationToken);
        if (existing is null)
        {
            dbContext.Vendors.Add(vendor);
            await dbContext.SaveChangesAsync(cancellationToken);
            return vendor;
        }

        existing.Name = vendor.Name;
        existing.ContactPerson = vendor.ContactPerson;
        existing.Phone = vendor.Phone;
        existing.Email = vendor.Email;
        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<IReadOnlyList<Part>> GetPartsAsync(CancellationToken cancellationToken = default) 
        => await dbContext.Parts.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.SalesInvoices.ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default) 
        => await dbContext.PurchaseInvoices.ToListAsync(cancellationToken);
}
