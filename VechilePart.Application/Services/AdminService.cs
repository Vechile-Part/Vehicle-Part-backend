using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;
using VehiclePart.Domain.Enums;
using VehiclePart.Domain.Entities;
namespace VehiclePart.Application.Services;

public class AdminService(IAdminRepository repository, INotificationService notificationService) : IAdminService
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

    public async Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) => await repository.GetAllUsersAsync(cancellationToken);

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken = default) => await repository.DeleteUserAsync(id, cancellationToken);

    public async Task<Part> AddPartAsync(AddPartDto dto, CancellationToken cancellationToken = default)
    {
        var part = new Part { Name = dto.Name, PartNumber = dto.PartNumber, UnitPrice = dto.UnitPrice, QuantityInStock = dto.QuantityInStock, VendorId = dto.VendorId };
        await repository.AddPartAsync(part, cancellationToken);
        return part;
    }

    public async Task<Part> UpdatePartAsync(Guid id, UpdatePartDto dto, CancellationToken cancellationToken = default)
    {
        var part = await repository.GetPartByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException("Part not found.");
        part.Name = dto.Name; part.PartNumber = dto.PartNumber; part.UnitPrice = dto.UnitPrice; part.QuantityInStock = dto.QuantityInStock; part.VendorId = dto.VendorId;
        await repository.UpdatePartAsync(part, cancellationToken);
        return part;
    }

    public async Task DeletePartAsync(Guid id, CancellationToken cancellationToken = default) => await repository.DeletePartAsync(id, cancellationToken);

    public async Task<IReadOnlyList<Part>> GetAllPartsAsync(CancellationToken cancellationToken = default) => await repository.GetLowStockPartsAsync(int.MaxValue, cancellationToken);

    public async Task PurchasePartAsync(Guid partId, int quantity, PurchasePartDto dto, CancellationToken cancellationToken = default)
    {
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

    public async Task<IReadOnlyList<object>> GetLowStockPartsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        var parts = await repository.GetLowStockPartsAsync(threshold, cancellationToken);
        return parts.Select(p => (object)new { p.Id, p.Name, p.PartNumber, p.QuantityInStock, p.UnitPrice, isLowStock = p.QuantityInStock < 10 }).ToList();
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
