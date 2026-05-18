using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Entities;

namespace VehiclePart.Application.Interfaces;

public interface IAdminService
{
    Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default);
    Task<FinancialDashboardDto> GetFinancialDashboardAsync(string period, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetOverdueCreditInvoicesAsync(int minimumAgeMonths, CancellationToken cancellationToken = default);
    Task RegisterStaffAsync(StaffRegistrationDto dto, CancellationToken cancellationToken = default);
    Task UpdateStaffRoleAsync(UpdateStaffRoleDto dto, CancellationToken cancellationToken = default);
    Task UpdateStaffDetailsAsync(UpdateStaffDetailsDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task DeleteStaffAsync(Guid userId, CancellationToken cancellationToken = default);
    Task DemoteStaffToCustomerAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetCustomerAccountsAsync(CancellationToken cancellationToken = default);
    Task PromoteCustomerAccountToStaffAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
    Task<Part> AddPartAsync(AddPartDto dto, CancellationToken cancellationToken = default);
    Task<Part> UpdatePartAsync(Guid id, UpdatePartDto dto, CancellationToken cancellationToken = default);
    Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartListItemDto>> GetAllPartsAsync(CancellationToken cancellationToken = default);
    Task<PagedPartsResultDto> GetPagedPartsAsync(int page, int pageSize, string? search = null, CancellationToken cancellationToken = default);
    Task PurchasePartAsync(Guid partId, int quantity, PurchasePartDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PartRequestAdminDto>> GetPartRequestsAsync(CancellationToken cancellationToken = default);
    Task UpdatePartRequestStatusAsync(Guid id, UpdatePartRequestStatusDto dto, CancellationToken cancellationToken = default);
}
