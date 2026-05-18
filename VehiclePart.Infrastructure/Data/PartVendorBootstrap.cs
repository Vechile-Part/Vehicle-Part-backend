using Microsoft.EntityFrameworkCore;

namespace VehiclePart.Infrastructure.Data;

public static class PartVendorBootstrap
{
    public static async Task RepairAsync(
        AppDbContext dbContext,
        bool assignSoleVendorToOrphans,
        CancellationToken cancellationToken = default)
    {
        var validVendorIds = (await dbContext.Vendors
            .AsNoTracking()
            .Select(vendor => vendor.Id)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        if (validVendorIds.Count == 0)
            return;

        var partsNeedingFix = await dbContext.Parts
            .Where(part => part.VendorId == Guid.Empty || !validVendorIds.Contains(part.VendorId))
            .ToListAsync(cancellationToken);

        if (partsNeedingFix.Count == 0)
            return;

        var latestVendorByPart = await (
            from item in dbContext.PurchaseInvoiceItems.AsNoTracking()
            join invoice in dbContext.PurchaseInvoices.AsNoTracking() on item.PurchaseInvoiceId equals invoice.Id
            where invoice.VendorId != Guid.Empty && validVendorIds.Contains(invoice.VendorId)
            orderby invoice.IssuedAtUtc descending
            select new { item.PartId, invoice.VendorId }
        ).ToListAsync(cancellationToken);

        var vendorFromPurchase = new Dictionary<Guid, Guid>();
        foreach (var row in latestVendorByPart)
        {
            if (!vendorFromPurchase.ContainsKey(row.PartId))
                vendorFromPurchase[row.PartId] = row.VendorId;
        }

        var defaultVendorId = assignSoleVendorToOrphans
            ? validVendorIds.OrderBy(id => id).First()
            : Guid.Empty;

        var changed = false;
        foreach (var part in partsNeedingFix)
        {
            if (vendorFromPurchase.TryGetValue(part.Id, out var fromPurchase))
            {
                part.VendorId = fromPurchase;
                changed = true;
                continue;
            }

            if (defaultVendorId != Guid.Empty)
            {
                part.VendorId = defaultVendorId;
                changed = true;
            }
        }

        if (changed)
            await dbContext.SaveChangesAsync(cancellationToken);
    }
}
