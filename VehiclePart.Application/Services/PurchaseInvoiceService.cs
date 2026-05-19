using VehiclePart.Application.DTOs.PurchaseInvoice;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Services;

public class PurchaseInvoiceService(
    IPurchaseInvoiceRepository purchaseInvoiceRepository,
    IAdminRepository adminRepository
) : IPurchaseInvoiceService
{
    public Task<PurchaseInvoiceResponseDto> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken cancellationToken = default) =>
        adminRepository.ExecuteInTransactionAsync(ct => CreateCoreAsync(dto, ct), cancellationToken);

    private async Task<PurchaseInvoiceResponseDto> CreateCoreAsync(
        CreatePurchaseInvoiceDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.Items.Count == 0)
            throw new ArgumentException("Purchase invoice must contain at least one item.");

        var invoice = new PurchaseInvoice
        {
            VendorId = dto.VendorId,
            IssuedAtUtc = DateTime.UtcNow,
            Items = []
        };

        decimal totalAmount = 0;
        var stockByPartId = new Dictionary<Guid, int>();

        foreach (var itemDto in dto.Items)
        {
            if (itemDto.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            if (itemDto.UnitPrice <= 0)
                throw new ArgumentException("Unit price must be greater than zero.");

            if (itemDto.PartId == Guid.Empty)
                throw new ArgumentException("Each line must reference a part.");

            if (!await adminRepository.PartExistsAsync(itemDto.PartId, cancellationToken))
                throw new KeyNotFoundException($"Part '{itemDto.PartId}' was not found.");

            stockByPartId[itemDto.PartId] = stockByPartId.GetValueOrDefault(itemDto.PartId) + itemDto.Quantity;
            totalAmount += itemDto.Quantity * itemDto.UnitPrice;

            invoice.Items.Add(new PurchaseInvoiceItem
            {
                PartId = itemDto.PartId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice
            });
        }

        foreach (var (partId, quantity) in stockByPartId)
        {
            var affected = await adminRepository.TryIncrementPartStockAsync(partId, quantity, cancellationToken);
            if (affected != 1)
                throw new InvalidOperationException($"Could not update stock for part '{partId}'.");

            if (dto.VendorId != Guid.Empty)
                await adminRepository.SetPartVendorAsync(partId, dto.VendorId, cancellationToken);
        }

        adminRepository.ClearChangeTracker();

        invoice.TotalAmount = totalAmount;

        var createdInvoice = await purchaseInvoiceRepository.CreateAsync(invoice, cancellationToken);

        return MapToResponse(createdInvoice);
    }

    public async Task<IReadOnlyList<PurchaseInvoiceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await purchaseInvoiceRepository.GetAllAsync(cancellationToken);
        return invoices.Select(MapToResponse).ToList();
    }

    public async Task<PurchaseInvoiceResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await purchaseInvoiceRepository.GetByIdAsync(id, cancellationToken);
        return invoice is null ? null : MapToResponse(invoice);
    }

    private static PurchaseInvoiceResponseDto MapToResponse(PurchaseInvoice invoice)
    {
        return new PurchaseInvoiceResponseDto
        {
            Id = invoice.Id,
            VendorId = invoice.VendorId,
            VendorName = invoice.Vendor?.Name ?? string.Empty,
            VendorContactPerson = invoice.Vendor?.ContactPerson ?? string.Empty,
            VendorPhone = invoice.Vendor?.Phone ?? string.Empty,
            VendorEmail = invoice.Vendor?.Email ?? string.Empty,
            IssuedAtUtc = invoice.IssuedAtUtc,
            TotalAmount = invoice.TotalAmount,
            Items = invoice.Items.Select(i => new PurchaseInvoiceItemDto
            {
                PartId = i.PartId,
                PartName = i.Part?.Name ?? string.Empty,
                PartNumber = i.Part?.PartNumber ?? string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
