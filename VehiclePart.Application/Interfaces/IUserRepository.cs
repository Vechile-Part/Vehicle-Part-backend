using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;

namespace VehiclePart.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task UpdateStaffCredentialsAsync(Guid userId, string passwordHash, RoleType role, CancellationToken cancellationToken = default);
}
