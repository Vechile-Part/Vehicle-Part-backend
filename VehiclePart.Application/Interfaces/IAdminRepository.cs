using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Entities;
namespace VehiclePart.Application.Interfaces;

public interface IAdminRepository
{
    Task<IReadOnlyList<SalesInvoice>> GetSalesInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseInvoice>> GetPurchaseInvoicesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Part>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Part Part, Guid EffectiveVendorId, string? VendorName)>> GetAllPartsWithVendorNamesAsync(
        CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<(Part Part, Guid EffectiveVendorId, string? VendorName)> Items, int TotalCount)> GetPagedPartsWithVendorNamesAsync(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OverdueCreditInvoiceDto>> GetOverdueCreditInvoicesAsync(
        int minimumAgeMonths,
        CancellationToken cancellationToken = default);
    Task AddUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SetUserPasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);
    Task AddUserPasswordSetupTokenAsync(UserPasswordSetupToken token, CancellationToken cancellationToken = default);
    Task<UserPasswordSetupToken?> GetActiveUserPasswordSetupTokenByHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task MarkUserPasswordSetupTokenUsedAsync(Guid tokenId, CancellationToken cancellationToken = default);
    Task InvalidateUnusedPasswordSetupTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetStaffUsersAsync(CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddPartAsync(Part part, CancellationToken cancellationToken = default);
    Task<Part?> GetPartByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> PartExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> GetPartUnitPriceAsync(Guid id, CancellationToken cancellationToken = default);
    void ClearChangeTracker();
    Task<int> TryIncrementPartStockAsync(Guid partId, int quantity, CancellationToken cancellationToken = default);
    Task SetPartVendorAsync(Guid partId, Guid vendorId, CancellationToken cancellationToken = default);
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default);
    Task UpdatePartAsync(Part part, CancellationToken cancellationToken = default);
    Task<bool> IsPartReferencedAsync(Guid partId, CancellationToken cancellationToken = default);
    Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddPurchaseInvoiceAsync(PurchaseInvoice invoice, CancellationToken cancellationToken = default);
    Task<string> ReserveNextPurchaseInvoiceNumberAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartRequestAdminDto>> GetPartRequestsAsync(CancellationToken cancellationToken = default);

    Task<PartRequest?> GetPartRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdatePartRequestAsync(PartRequest partRequest, CancellationToken cancellationToken = default);

    Task RepairPartVendorLinksAsync(CancellationToken cancellationToken = default);
}