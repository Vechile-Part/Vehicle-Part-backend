using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/staff")]
public class StaffController(IAdminService adminService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto, CancellationToken cancellationToken)
    {
        await adminService.RegisterStaffAsync(dto, cancellationToken);
        return Ok("Staff registered successfully.");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllStaff(CancellationToken cancellationToken) => Ok(await adminService.GetAllUsersAsync(cancellationToken));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(Guid id, CancellationToken cancellationToken)
    {
        await adminService.DeleteUserAsync(id, cancellationToken);
        return Ok("Staff deleted successfully.");
    }

    [HttpPut("role")]
    public async Task<IActionResult> UpdateStaffRole([FromBody] UpdateStaffRoleDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffRoleAsync(dto, cancellationToken);
        return Ok("Role updated successfully.");
    }

    [HttpPut("details")]
    public async Task<IActionResult> UpdateStaffDetails([FromBody] UpdateStaffDetailsDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffDetailsAsync(dto, cancellationToken);
        return Ok("Staff details updated successfully.");
    }
}