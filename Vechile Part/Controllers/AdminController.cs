using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.DTOs;
using VechilePart.Application.Interfaces;
using VechilePart.Domain.Entities;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase 
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) 
    { 
        _adminService = adminService; 
    }

    [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto) 
    {
        await _adminService.RegisterStaffAsync(dto);
        return Ok("Staff registered successfully");
    }

    [HttpGet("parts")]
    public async Task<IActionResult> GetParts() 
    {
        var parts = await _adminService.GetAllPartsAsync();
        return Ok(parts);
    }

    [HttpPost("parts")]
    public async Task<IActionResult> AddPart([FromBody] Part part) 
    {
        await _adminService.ManagePartAsync(part);
        return Ok(part);
    }
}