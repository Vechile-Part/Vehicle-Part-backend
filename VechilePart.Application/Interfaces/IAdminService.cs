using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Entities;
namespace VehiclePart.Application.Interfaces;

public interface IAdminService
{
    Task<FinancialReportDto> GetFinancialReportAsync(string reportType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<object>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default);
    Task RegisterStaffAsync(StaffRegistrationDto dto, CancellationToken cancellationToken = default);
    Task UpdateStaffRoleAsync(UpdateStaffRoleDto dto, CancellationToken cancellationToken = default);
    Task UpdateStaffDetailsAsync(UpdateStaffDetailsDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Part> AddPartAsync(AddPartDto dto, CancellationToken cancellationToken = default);
    Task<Part> UpdatePartAsync(Guid id, UpdatePartDto dto, CancellationToken cancellationToken = default);
    Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Part>> GetAllPartsAsync(CancellationToken cancellationToken = default);
    Task PurchasePartAsync(Guid partId, int quantity, PurchasePartDto dto, CancellationToken cancellationToken = default);
}