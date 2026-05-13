using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Entities;
namespace VehiclePart.Application.Interfaces;

public interface IAdminRepository
{
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Part>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OverdueCreditInvoiceDto>> GetOverdueCreditInvoicesAsync(
        int minimumAgeMonths,
        CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddPartAsync(Part part, CancellationToken cancellationToken = default);
    Task<Part?> GetPartByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdatePartAsync(Part part, CancellationToken cancellationToken = default);
    Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default);
}