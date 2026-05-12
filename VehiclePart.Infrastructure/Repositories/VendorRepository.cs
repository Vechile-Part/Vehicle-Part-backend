using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Infrastructure.Data;

namespace VehiclePart.Infrastructure.Repositories;

public class VendorRepository(AppDbContext dbContext) : IVendorRepository
{
    public async Task<IReadOnlyList<Vendor>> GetAllVendorsAsync(CancellationToken cancellationToken = default)
        => await dbContext.Vendors.ToListAsync(cancellationToken);

    public async Task<Vendor?> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Vendors.FindAsync([id], cancellationToken);

    public async Task<Vendor> AddVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        dbContext.Vendors.Add(vendor);
        await dbContext.SaveChangesAsync(cancellationToken);
        return vendor;
    }

    public async Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken = default)
    {
        dbContext.Vendors.Update(vendor);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteVendorAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vendor = await dbContext.Vendors.FindAsync([id], cancellationToken);
        if (vendor is not null)
        {
            dbContext.Vendors.Remove(vendor);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
