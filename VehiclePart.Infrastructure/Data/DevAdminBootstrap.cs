using Microsoft.EntityFrameworkCore;
using VehiclePart.Application.Security;
using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;

namespace VehiclePart.Infrastructure.Data;

public static class DevAdminBootstrap
{
    private static readonly Guid DevAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private const string DevAdminEmail = "admin.vehiclepart@gmail.com";
    private const string DevAdminPassword = "admin123@";

    public static async Task EnsureAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var admin = await dbContext.Users
            .FirstOrDefaultAsync(
                u => u.Id == DevAdminId
                     || u.Email.ToLower() == DevAdminEmail.ToLower()
                     || u.Email.ToLower() == "admin@vehiclepart.com",
                cancellationToken);

        if (admin is null)
        {
            dbContext.Users.Add(new User
            {
                Id = DevAdminId,
                FullName = "Admin",
                Email = DevAdminEmail,
                Phone = "9800000000",
                Password = CustomerPasswordHasher.HashPassword(DevAdminPassword),
                Role = RoleType.Admin
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var passwordValid = CustomerPasswordHasher.LooksLikeHash(admin.Password)
            ? CustomerPasswordHasher.VerifyPassword(DevAdminPassword, admin.Password)
            : admin.Password == DevAdminPassword;

        if (!passwordValid || admin.Role != RoleType.Admin)
        {
            admin.Password = CustomerPasswordHasher.HashPassword(DevAdminPassword);
            admin.Role = RoleType.Admin;
        }

        if (!string.Equals(admin.Email, DevAdminEmail, StringComparison.OrdinalIgnoreCase))
            admin.Email = DevAdminEmail;

        if (!CustomerPasswordHasher.LooksLikeHash(admin.Password))
            admin.Password = CustomerPasswordHasher.HashPassword(DevAdminPassword);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
