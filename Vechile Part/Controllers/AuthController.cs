using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VehiclePart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext context, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        // Check Users table first (Admin/Staff)
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user != null)
        {
            if (user.Password != dto.Password)
                return Unauthorized("Invalid email or password.");

            return Ok(new { token = GenerateToken(user.Id, user.Email, user.Role.ToString()), role = user.Role.ToString() });
        }

        // Check Customers table
        var customer = await context.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
        if (customer != null)
        {
            Console.WriteLine($"[DEBUG] User found: {user != null}");
            Console.WriteLine($"[DEBUG] Customer found: {customer != null}");
            Console.WriteLine($"[DEBUG] Hash from DB: {customer.PasswordHash}");
            Console.WriteLine($"[DEBUG] Parts count: {customer.PasswordHash.Split('.').Length}");
            var result = VerifyPassword(dto.Password, customer.PasswordHash);
            Console.WriteLine($"[DEBUG] Verify result: {result}");
            if (!result)
                return Unauthorized("Invalid email or password.");

            return Ok(new { token = GenerateToken(customer.Id, customer.Email, "Customer"), role = "Customer" });
        }

        return Unauthorized("Invalid email or password.");
    }

    private string GenerateToken(Guid id, string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(config["JWT:Secret"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("UserId", id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = config["JWT:Issuer"],
            Audience = config["JWT:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split('.');
        if (parts.Length != 3) return false;

        int iterations = int.Parse(parts[0]);
        byte[] salt = Convert.FromBase64String(parts[1]);
        byte[] expectedHash = Convert.FromBase64String(parts[2]);

        byte[] actualHash = System.Security.Cryptography.Rfc2898DeriveBytes.Pbkdf2(
            System.Text.Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);

        return expectedHash.SequenceEqual(actualHash);
    }
}

public record LoginDto(string Email, string Password);