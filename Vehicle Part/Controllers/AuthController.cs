using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Domain.Entities;
using VehiclePart.Domain.Enums;
using VehiclePart.Infrastructure.Data;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext context,
    ICustomerAuthService customerAuthService,
    ICustomerInviteService customerInviteService,
    IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        if (user == null || user.Password != dto.Password)
            return Unauthorized("Invalid email or password.");

        if (!TryResolveStaffUserRole(user, out var resolvedRole))
            return Unauthorized("Invalid email or password.");

        if (user.Role != resolvedRole)
        {
            user.Role = resolvedRole;
            await context.SaveChangesAsync(cancellationToken);
        }

        var roleClaim = UserRoleClaimValue(resolvedRole);
        var token = CreateJwtToken(
        [
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, roleClaim),
            new Claim("UserId", user.Id.ToString())
        ]);

        return Ok(new { token, role = roleClaim });
    }

    [HttpPost("customer/login")]
    public async Task<IActionResult> CustomerLogin([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var auth = await customerAuthService.ValidateCredentialsAsync(dto.Email, dto.Password, cancellationToken);
        if (auth is null)
            return Unauthorized("Invalid email or password.");

        var token = CreateJwtToken(
        [
            new Claim(ClaimTypes.Name, auth.Email),
            new Claim(ClaimTypes.Role, "Customer"),
            new Claim("CustomerId", auth.Id.ToString())
        ]);

        return Ok(new { token, role = "Customer", customerId = auth.Id });
    }

    [AllowAnonymous]
    [HttpPost("customer/complete-invite-password")]
    public async Task<IActionResult> CompleteCustomerPasswordInvite(
        [FromBody] CompleteCustomerPasswordInviteDto dto,
        CancellationToken cancellationToken)
    {
        var (ok, error) = await customerInviteService.TryCompletePasswordInviteAsync(
            dto.Token,
            dto.NewPassword,
            cancellationToken);

        if (!ok)
            return BadRequest(new { message = error ?? "Request could not be completed." });

        return Ok(new { message = "Password saved. You can sign in now." });
    }

    /// <summary>
    /// Resolves staff/admin role. Repairs legacy rows where <see cref="RoleType"/> was stored as 0
    /// (so <see cref="RoleType.ToString"/> produced "0" and broke clients).
    /// </summary>
    private static bool TryResolveStaffUserRole(User user, out RoleType resolved)
    {
        if (user.Role is RoleType.Admin or RoleType.Staff)
        {
            resolved = user.Role;
            return true;
        }

        if (string.Equals(user.Email, "admin@vehiclepart.com", StringComparison.OrdinalIgnoreCase))
        {
            resolved = RoleType.Admin;
            return true;
        }

        resolved = default;
        return false;
    }

    private static string UserRoleClaimValue(RoleType role) =>
        role switch
        {
            RoleType.Admin => "Admin",
            RoleType.Staff => "Staff",
            _ => throw new InvalidOperationException($"Unexpected staff user role: {role}")
        };

    private string CreateJwtToken(IEnumerable<Claim> claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(config["JWT:Secret"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = config["JWT:Issuer"],
            Audience = config["JWT:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public record LoginDto(string Email, string Password);
