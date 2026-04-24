using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;
using VechilePart.Domain.Entities;
using VechilePart.Domain.Enums;

namespace VechilePart.Application.Services;

public class AdminService(IAdminRepository repository) : IAdminService
{
    public async Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default)
    {
        var sales = await repository.GetSalesInvoicesAsync(cancellationToken);
        var purchases = await repository.GetPurchaseInvoicesAsync(cancellationToken);

        return new FinancialReportDto(
            reportType,
            sales.Sum(x => x.TotalAmount),
            purchases.Sum(x => x.TotalAmount),
            sales.Sum(x => x.PendingCredit));
    }

    public async Task RegisterStaffAsync(StaffRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        _ = await repository.AddUserAsync(new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Role = Enum.TryParse<RoleType>(dto.Role, true, out var role) ? role : RoleType.Staff
        }, cancellationToken);
    }

    public async Task<PartDto> UpsertPartAsync(PartDto dto, CancellationToken cancellationToken = default)
    {
        var part = await repository.UpsertPartAsync(new Part
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            PartNumber = dto.PartNumber,
            UnitPrice = dto.UnitPrice,
            QuantityInStock = dto.QuantityInStock,
            VendorId = dto.VendorId
        }, cancellationToken);

        return new PartDto(part.Id, part.Name, part.PartNumber, part.UnitPrice, part.QuantityInStock, part.VendorId);
    }

    public Task DeletePartAsync(Guid partId, CancellationToken cancellationToken = default) => repository.DeletePartAsync(partId, cancellationToken);

    public async Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(PurchaseInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        var invoice = await repository.AddPurchaseInvoiceAsync(new PurchaseInvoice
        {
            VendorId = dto.VendorId,
            TotalAmount = dto.TotalAmount
        }, cancellationToken);

        return new PurchaseInvoiceDto(invoice.Id, invoice.VendorId, invoice.TotalAmount, invoice.IssuedAtUtc);
    }

    public async Task<VendorDto> UpsertVendorAsync(VendorDto dto, CancellationToken cancellationToken = default)
    {
        var vendor = await repository.UpsertVendorAsync(new Vendor
        {
            Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email
        }, cancellationToken);

        return new VendorDto(vendor.Id, vendor.Name, vendor.ContactPerson, vendor.Phone, vendor.Email);
    }

    public async Task<IReadOnlyList<PartDto>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        var parts = await repository.GetPartsAsync(cancellationToken);
        return parts
            .Where(p => p.QuantityInStock < threshold)
            .Select(p => new PartDto(p.Id, p.Name, p.PartNumber, p.UnitPrice, p.QuantityInStock, p.VendorId))
            .ToList();
    }
}
