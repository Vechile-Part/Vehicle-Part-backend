using Microsoft.EntityFrameworkCore;

namespace VehiclePart.Infrastructure.Data;

public static class InvoiceNumberBootstrap
{
    public static async Task BackfillMissingAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var missing = await dbContext.SalesInvoices
            .Where(invoice => invoice.InvoiceNumber == null || invoice.InvoiceNumber == string.Empty)
            .OrderBy(invoice => invoice.IssuedAtUtc)
            .ToListAsync(cancellationToken);

        if (missing.Count == 0)
            return;

        var sequencesByYear = await dbContext.SalesInvoices.AsNoTracking()
            .Where(invoice => invoice.InvoiceNumber != null && invoice.InvoiceNumber != string.Empty)
            .Select(invoice => invoice.InvoiceNumber)
            .ToListAsync(cancellationToken);

        var yearCounters = new Dictionary<int, int>();
        foreach (var number in sequencesByYear)
        {
            if (!TryParseInvoiceSequence(number, out var year, out var sequence))
                continue;

            if (!yearCounters.TryGetValue(year, out var current) || sequence > current)
                yearCounters[year] = sequence;
        }

        foreach (var invoice in missing)
        {
            var year = invoice.IssuedAtUtc.Year;
            if (!yearCounters.TryGetValue(year, out var next))
                next = 0;
            next++;
            yearCounters[year] = next;
            invoice.InvoiceNumber = $"INV-{year}-{next:D3}";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool TryParseInvoiceSequence(string number, out int year, out int sequence)
    {
        year = 0;
        sequence = 0;
        if (!number.StartsWith("INV-", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = number.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
            return false;

        return int.TryParse(parts[1], out year) && int.TryParse(parts[2], out sequence);
    }
}
