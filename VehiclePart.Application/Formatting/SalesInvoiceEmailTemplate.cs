using System.Net;
using System.Text;
using VehiclePart.Application.Common;

namespace VehiclePart.Application.Formatting;

public sealed record SalesInvoiceEmailLine(string PartName, int Quantity, decimal UnitPrice, decimal LineTotal);

public static class SalesInvoiceEmailTemplate
{
    private const string BrandName = "PartTrack";

    public static string Build(
        string customerName,
        string invoiceNumber,
        DateTime issuedAtUtc,
        IReadOnlyList<SalesInvoiceEmailLine> lines,
        decimal subtotalBeforeDiscount,
        decimal discountAmount,
        decimal totalAmount,
        decimal paidAmount,
        decimal pendingCredit)
    {
        var safeName = Encode(customerName);
        var safeNumber = Encode(invoiceNumber);
        var issuedLabel = Encode(NepalClock.FormatLocalDateTime(issuedAtUtc));

        return Wrap(
            $"""
            <p style="margin:0 0 16px;font-size:15px;">Hello <strong>{safeName}</strong>,</p>
            <p style="margin:0 0 20px;font-size:14px;color:#555;line-height:1.5;">
              Please find your sales invoice below.
            </p>
            <p style="margin:0 0 20px;font-size:14px;">
              <strong>Invoice:</strong> {safeNumber}<br />
              <strong>Date:</strong> {issuedLabel}
            </p>
            {BuildItemsTable(lines)}
            {BuildTotalsBlock(subtotalBeforeDiscount, discountAmount, totalAmount, paidAmount, pendingCredit)}
            {BuildBalanceNote(pendingCredit)}
            <p style="margin:24px 0 0;font-size:14px;color:#555;">Thank you for your business.</p>
            """);
    }

    public static string BuildSummaryOnly(
        string customerName,
        string invoiceReference,
        decimal totalAmount,
        decimal paidAmount,
        decimal pendingCredit)
    {
        var safeName = Encode(customerName);
        var safeRef = Encode(invoiceReference);

        return Wrap(
            $"""
            <p style="margin:0 0 16px;font-size:15px;">Hello <strong>{safeName}</strong>,</p>
            <p style="margin:0 0 20px;font-size:14px;"><strong>Reference:</strong> {safeRef}</p>
            {BuildTotalsBlock(totalAmount, 0, totalAmount, paidAmount, pendingCredit)}
            {BuildBalanceNote(pendingCredit)}
            <p style="margin:24px 0 0;font-size:14px;color:#555;">Thank you.</p>
            """);
    }

    private static string Wrap(string inner) =>
        $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
        </head>
        <body style="margin:0;padding:16px;font-family:Arial,Helvetica,sans-serif;font-size:14px;color:#222;background:#f5f5f5;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;margin:0 auto;background:#fff;border:1px solid #ddd;">
            <tr>
              <td style="padding:20px 24px;border-bottom:1px solid #eee;">
                <p style="margin:0;font-size:18px;font-weight:bold;color:#333;">{BrandName}</p>
                <p style="margin:4px 0 0;font-size:13px;color:#666;">Sales invoice</p>
              </td>
            </tr>
            <tr>
              <td style="padding:24px;">
                {inner}
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

    private static string BuildItemsTable(IReadOnlyList<SalesInvoiceEmailLine> lines)
    {
        if (lines.Count == 0)
        {
            return """<p style="margin:0 0 20px;color:#666;">No line items on this invoice.</p>""";
        }

        var rows = new StringBuilder();
        foreach (var line in lines)
        {
            rows.Append(
                $"""
                <tr>
                  <td style="padding:8px 10px;border-bottom:1px solid #eee;">{Encode(line.PartName)}</td>
                  <td align="center" style="padding:8px 6px;border-bottom:1px solid #eee;">{line.Quantity}</td>
                  <td align="right" style="padding:8px 6px;border-bottom:1px solid #eee;">{Encode(NprFormatter.Format(line.UnitPrice))}</td>
                  <td align="right" style="padding:8px 10px;border-bottom:1px solid #eee;">{Encode(NprFormatter.Format(line.LineTotal))}</td>
                </tr>
                """);
        }

        return $"""
        <table width="100%" cellpadding="0" cellspacing="0" style="border-collapse:collapse;margin:0 0 20px;border:1px solid #ddd;">
          <thead>
            <tr style="background:#f7f7f7;">
              <th align="left" style="padding:8px 10px;font-size:12px;border-bottom:1px solid #ddd;">Part</th>
              <th align="center" style="padding:8px 6px;font-size:12px;border-bottom:1px solid #ddd;">Qty</th>
              <th align="right" style="padding:8px 6px;font-size:12px;border-bottom:1px solid #ddd;">Unit</th>
              <th align="right" style="padding:8px 10px;font-size:12px;border-bottom:1px solid #ddd;">Total</th>
            </tr>
          </thead>
          <tbody>{rows}</tbody>
        </table>
        """;
    }

    private static string BuildTotalsBlock(
        decimal subtotalBeforeDiscount,
        decimal discountAmount,
        decimal totalAmount,
        decimal paidAmount,
        decimal pendingCredit)
    {
        var rows = new StringBuilder();
        rows.Append(TotalLine("Subtotal", subtotalBeforeDiscount));

        if (discountAmount > 0)
            rows.Append(TotalLine("Discount", -discountAmount));

        rows.Append(TotalLine("Total", totalAmount, bold: true));
        rows.Append(TotalLine("Paid", paidAmount));
        rows.Append(TotalLine("Balance due", pendingCredit, bold: pendingCredit > 0));

        return $"""
        <table width="100%" cellpadding="0" cellspacing="0" style="margin:0 0 12px;">
          {rows}
        </table>
        """;
    }

    private static string TotalLine(string label, decimal amount, bool bold = false)
    {
        var display = amount < 0
            ? $"− {NprFormatter.Format(Math.Abs(amount))}"
            : NprFormatter.Format(amount);

        var weight = bold ? "font-weight:bold;" : "";
        return $"""
        <tr>
          <td style="padding:6px 0;color:#555;{weight}">{Encode(label)}</td>
          <td align="right" style="padding:6px 0;{weight}">{Encode(display)}</td>
        </tr>
        """;
    }

    private static string BuildBalanceNote(decimal pendingCredit)
    {
        if (pendingCredit <= 0)
            return string.Empty;

        return $"""
        <p style="margin:0;font-size:13px;color:#666;">
          Please pay the remaining <strong>{Encode(NprFormatter.Format(pendingCredit))}</strong> at our counter when convenient.
        </p>
        """;
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
