namespace VehiclePart.Application.DTOs;

public record FinancialSalesLedgerRow(DateTime IssuedAtUtc, decimal TotalAmount, decimal PendingCredit);

public record FinancialPurchaseLedgerRow(DateTime IssuedAtUtc, decimal TotalAmount);
