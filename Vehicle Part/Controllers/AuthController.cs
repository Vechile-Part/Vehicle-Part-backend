using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.Security;
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
        if (dto is null || string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email and password are required.");

        var normalizedEmail = dto.Email.Trim();
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == normalizedEmail.ToLower(),
            cancellationToken);

        if (user is null || !TryVerifyStaffPassword(user, dto.Password, out var mustRehash))
            return Unauthorized("Invalid email or password.");

        if (mustRehash)
        {
            user.Password = CustomerPasswordHasher.HashPassword(dto.Password);
            await context.SaveChangesAsync(cancellationToken);
        }

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

    private static bool TryResolveStaffUserRole(User user, out RoleType resolved)
    {
        if (user.Role is RoleType.Admin or RoleType.Staff)
        {
            resolved = user.Role;
            return true;
        }

        if (string.Equals(user.Email, "admin.vehiclepart@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            resolved = RoleType.Admin;
            return true;
        }

        resolved = default;
        return false;
    }

    private static bool TryVerifyStaffPassword(User user, string password, out bool mustRehash)
    {
        mustRehash = false;
        if (CustomerPasswordHasher.LooksLikeHash(user.Password))
            return CustomerPasswordHasher.VerifyPassword(password, user.Password);

        if (user.Password == password)
        {
            mustRehash = true;
            return true;
        }

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
