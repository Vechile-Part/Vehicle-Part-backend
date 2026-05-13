using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Enums;
using VehiclePart.Domain.Entities;
namespace VehiclePart.Application.Services;

public class AdminService(IAdminRepository repository) : IAdminService
{
    public async Task RegisterStaffAsync(StaffRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var user = new User { FullName = dto.FullName, Email = dto.Email, Phone = dto.Phone, Password = dto.Password, Role = RoleType.Staff };
        await repository.AddUserAsync(user, cancellationToken);
    }

    public async Task UpdateStaffRoleAsync(UpdateStaffRoleDto dto, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetUserByIdAsync(dto.UserId, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        user.Role = dto.NewRole;
        await repository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task UpdateStaffDetailsAsync(UpdateStaffDetailsDto dto, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetUserByIdAsync(dto.UserId, cancellationToken) ?? throw new KeyNotFoundException("User not found.");
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Phone = dto.Phone;
        await repository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task<Part> AddPartAsync(AddPartDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.QuantityInStock < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(dto));

        var part = new Part { Name = dto.Name, PartNumber = dto.PartNumber, UnitPrice = dto.UnitPrice, QuantityInStock = dto.QuantityInStock, VendorId = dto.VendorId };
        await repository.AddPartAsync(part, cancellationToken);
        return part;
    }

    public async Task<Part> UpdatePartAsync(Guid id, UpdatePartDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.QuantityInStock < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(dto));

        var part = await repository.GetPartByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Part not found.");
        part.Name = dto.Name; part.PartNumber = dto.PartNumber; part.UnitPrice = dto.UnitPrice; part.QuantityInStock = dto.QuantityInStock; part.VendorId = dto.VendorId;
        await repository.UpdatePartAsync(part, cancellationToken);
        return part;
    }

    public async Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default) => await repository.DeletePartAsync(id, cancellationToken);

    public async Task<IReadOnlyList<object>> GetAllPartsAsync(CancellationToken cancellationToken = default)
    {
        var parts = await repository.GetLowStockPartsAsync(int.MaxValue, cancellationToken);
        return parts.Select(p => (object)new
        {
            p.Id,
            p.Name,
            p.PartNumber,
            p.UnitPrice,
            p.QuantityInStock,
            isLowStock = p.QuantityInStock < 10
        }).ToList();
    }

    public async Task PurchasePartAsync(Guid partId, int quantity, PurchasePartDto dto, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new ArgumentException("Purchase quantity must be positive.", nameof(quantity));

        var part = await repository.GetPartByIdAsync(partId, cancellationToken) ?? throw new KeyNotFoundException("Part not found.");
        part.QuantityInStock += quantity;
        await repository.UpdatePartAsync(part, cancellationToken);
        await repository.AddPurchaseInvoiceAsync(new PurchaseInvoice { VendorId = dto.VendorId, TotalAmount = dto.TotalAmount, IssuedAtUtc = DateTime.UtcNow }, cancellationToken);
    }

    public async Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default)
    {
        var start = reportType.ToLower() switch { "daily" => DateTime.UtcNow.Date, "monthly" => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1), "yearly" => new DateTime(DateTime.UtcNow.Year, 1, 1), _ => DateTime.UtcNow.Date };
        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);
        var purchases = await repository.GetPurchaseInvoicesAsync(cancellationToken);
        return new FinancialReportDto(reportType, sales.Where(x => x.IssuedAtUtc >= start).Sum(x => x.TotalAmount), purchases.Where(x => x.IssuedAtUtc >= start).Sum(x => x.TotalAmount), sales.Where(x => x.IssuedAtUtc >= start).Sum(x => x.PendingCredit));
    }

    public async Task<FinancialDashboardDto> GetFinancialDashboardAsync(string period, CancellationToken cancellationToken = default)
    {
        var sales = (await repository.GetSalesInvoicesAsync(cancellationToken)).ToList();
        var purchases = (await repository.GetPurchaseInvoicesAsync(cancellationToken)).ToList();
        var p = (period ?? "daily").ToLowerInvariant();
        var now = DateTime.UtcNow;

        IReadOnlyList<FinancialBucketDto> chartBuckets;
        IReadOnlyList<FinancialBucketDto> tableRows;

        if (p == "yearly")
        {
            chartBuckets = BuildMonthBuckets(sales, purchases, now, 7);
            tableRows = chartBuckets;
        }
        else if (p == "monthly")
        {
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var start = monthStart > now.Date.AddDays(-6) ? monthStart : now.Date.AddDays(-6);
            chartBuckets = BuildDayBuckets(sales, purchases, start, now.Date, "ddd");
            tableRows = BuildDayBuckets(sales, purchases, start, now.Date, "MMM dd, yyyy");
        }
        else
        {
            chartBuckets = BuildDayBuckets(sales, purchases, now.Date.AddDays(-6), now.Date, "ddd");
            tableRows = BuildDayBuckets(sales, purchases, now.Date.AddDays(-6), now.Date, "MMM dd, yyyy");
        }

        var totalNet = chartBuckets.Sum(b => b.NetProfit);
        var prevNet = ComputePreviousWindowNet(sales, purchases, p, now);
        var estimatedTax = Math.Round(Math.Max(0, totalNet) * 0.196m, 2, MidpointRounding.AwayFromZero);
        var pendingInvoices = sales.Count(x => x.PendingCredit > 0);
        var totalPending = sales.Where(x => x.PendingCredit > 0).Sum(x => x.PendingCredit);

        return new FinancialDashboardDto(
            p,
            chartBuckets,
            tableRows,
            totalNet,
            prevNet,
            estimatedTax,
            pendingInvoices,
            totalPending);
    }

    private static decimal ComputePreviousWindowNet(List<SalesInvoice> sales, List<PurchaseInvoice> purchases, string period, DateTime now)
    {
        if (period == "yearly")
        {
            var firstCurrent = new DateTime(now.Year, now.Month, 1).AddMonths(-6);
            var prevStart = firstCurrent.AddMonths(-7);
            var prevEnd = firstCurrent.AddTicks(-1);
            return NetForRange(sales, purchases, prevStart, prevEnd);
        }

        var endPrev = now.Date.AddDays(-7);
        var startPrev = now.Date.AddDays(-13);
        return NetForRange(sales, purchases, startPrev, endPrev.AddDays(1).AddTicks(-1));
    }

    private static decimal NetForRange(List<SalesInvoice> sales, List<PurchaseInvoice> purchases, DateTime start, DateTime end)
    {
        var rev = sales.Where(x => x.IssuedAtUtc >= start && x.IssuedAtUtc <= end).Sum(x => x.TotalAmount);
        var cost = purchases.Where(x => x.IssuedAtUtc >= start && x.IssuedAtUtc <= end).Sum(x => x.TotalAmount);
        return rev - cost;
    }

    private static IReadOnlyList<FinancialBucketDto> BuildDayBuckets(
        List<SalesInvoice> sales,
        List<PurchaseInvoice> purchases,
        DateTime firstDay,
        DateTime lastDay,
        string labelFormat)
    {
        var list = new List<FinancialBucketDto>();
        for (var d = firstDay; d <= lastDay; d = d.AddDays(1))
        {
            var next = d.AddDays(1);
            var rev = sales.Where(x => x.IssuedAtUtc >= d && x.IssuedAtUtc < next).Sum(x => x.TotalAmount);
            var cost = purchases.Where(x => x.IssuedAtUtc >= d && x.IssuedAtUtc < next).Sum(x => x.TotalAmount);
            var net = rev - cost;
            list.Add(new FinancialBucketDto(
                d.ToString(labelFormat),
                d,
                rev,
                cost,
                net,
                MarginStatus(rev, net)));
        }

        return list;
    }

    private static IReadOnlyList<FinancialBucketDto> BuildMonthBuckets(List<SalesInvoice> sales, List<PurchaseInvoice> purchases, DateTime now, int count)
    {
        var list = new List<FinancialBucketDto>();
        var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-(count - 1));
        for (var i = 0; i < count; i++)
        {
            var monthStart = startMonth.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1);
            var rev = sales.Where(x => x.IssuedAtUtc >= monthStart && x.IssuedAtUtc < monthEnd).Sum(x => x.TotalAmount);
            var cost = purchases.Where(x => x.IssuedAtUtc >= monthStart && x.IssuedAtUtc < monthEnd).Sum(x => x.TotalAmount);
            var net = rev - cost;
            list.Add(new FinancialBucketDto(
                monthStart.ToString("MMM yyyy"),
                monthStart,
                rev,
                cost,
                net,
                MarginStatus(rev, net)));
        }

        return list;
    }

    private static string MarginStatus(decimal revenue, decimal net)
    {
        if (revenue <= 0)
            return "No Sales";
        if (net < revenue * 0.15m)
            return "Low Margin";
        return "Completed";
    }

    public async Task<IReadOnlyList<object>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        var parts = await repository.GetLowStockPartsAsync(threshold, cancellationToken);
        return parts.Select(p => (object)new { p.Id, p.Name, p.PartNumber, p.QuantityInStock, p.UnitPrice, isLowStock = p.QuantityInStock < 10 }).ToList();
    }

    public async Task<IReadOnlyList<object>> GetOverdueCreditInvoicesAsync(int minimumAgeMonths, CancellationToken cancellationToken = default)
    {
        var rows = await repository.GetOverdueCreditInvoicesAsync(minimumAgeMonths, cancellationToken);
        var now = DateTime.UtcNow;
        return rows
            .Select(r => (object)new
            {
                invoiceId = r.InvoiceId,
                customerId = r.CustomerId,
                customerName = r.CustomerName,
                customerEmail = r.CustomerEmail,
                pendingCredit = r.PendingCredit,
                issuedAtUtc = r.IssuedAtUtc,
                lastReminderSentUtc = r.LastReminderSentUtc,
                daysOutstanding = (int)Math.Floor((now - r.IssuedAtUtc).TotalDays)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<object>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await repository.GetAllUsersAsync(cancellationToken);
        return users.Select(u => (object)new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.Phone,
            u.Role
        }).ToList();
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await repository.DeleteUserAsync(id, cancellationToken);
    }
}
