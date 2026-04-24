using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

namespace VechilePart.Infrastructure.Repositories;

public class AdminRepository(AppDbContext dbContext) : IAdminRepository
{
    public Task<User> AddUserAsync(User user, CancellationToken cancellationToken = default)
    {
        dbContext.Users.Add(user);
        return Task.FromResult(user);
    }

    public Task<Part> UpsertPartAsync(Part part, CancellationToken cancellationToken = default)
    {
        var existing = dbContext.Parts.FirstOrDefault(x => x.Id == part.Id);
        if (existing is null)
        {
            dbContext.Parts.Add(part);
            return Task.FromResult(part);
        }

        existing.Name = part.Name;
        existing.PartNumber = part.PartNumber;
        existing.UnitPrice = part.UnitPrice;
        existing.QuantityInStock = part.QuantityInStock;
        existing.VendorId = part.VendorId;
        return Task.FromResult(existing);
    }

    public Task DeletePartAsync(Guid partId, CancellationToken cancellationToken = default)
    {
        dbContext.Parts.RemoveAll(x => x.Id == partId);
        return Task.CompletedTask;
    }

    public Task<PurchaseInvoice> AddPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default)
    {
        dbContext.PurchaseInvoices.Add(invoice);
        return Task.FromResult(invoice);
    }

    public Task<Vendor> UpsertVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        var existing = dbContext.Vendors.FirstOrDefault(x => x.Id == vendor.Id);
        if (existing is null)
        {
            dbContext.Vendors.Add(vendor);
            return Task.FromResult(vendor);
        }

        existing.Name = vendor.Name;
        existing.ContactPerson = vendor.ContactPerson;
        existing.Phone = vendor.Phone;
        existing.Email = vendor.Email;
        return Task.FromResult(existing);
    }

    public Task<IReadOnlyList<Part>> GetPartsAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<Part>)dbContext.Parts);
    public Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<SalesInvoice>)dbContext.SalesInvoices);
    public Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default) => Task.FromResult((IReadOnlyList<PurchaseInvoice>)dbContext.PurchaseInvoices);
}
