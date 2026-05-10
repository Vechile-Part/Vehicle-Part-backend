using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehiclePart.Application.Interfaces;
using VehiclePart.Infrastructure.Data;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    AppDbContext context,
    ICustomerAuthService customerAuthService,
    IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        if (user == null || user.Password != dto.Password)
            return Unauthorized("Invalid email or password.");

        var token = CreateJwtToken(
        [
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("UserId", user.Id.ToString())
        ]);

        return Ok(new { token, role = user.Role.ToString() });
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
