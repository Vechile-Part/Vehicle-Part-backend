using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VechilePart.Domain.Entities;
using VechilePart.Application.DTOs;
using VechilePart.Infrastructure.Data; 

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase {
    private readonly AppDbContext _context; 

    public AdminController(AppDbContext context) { _context = context; }

    [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto) {
        var staff = new User { Name = dto.Name, Email = dto.Email, PasswordHash = dto.Password, Role = UserRole.Staff };
        _context.Users.Add(staff);
        await _context.SaveChangesAsync();
        return Ok(staff);
    }

    [HttpGet("parts")]
    public async Task<IActionResult> GetParts() {
        return Ok(await _context.Parts.ToListAsync());
    }

    [HttpPost("parts")]
    public async Task<IActionResult> AddPart([FromBody] Part part) {
        _context.Parts.Add(part);
        await _context.SaveChangesAsync();
        return Ok(part);
    }
}