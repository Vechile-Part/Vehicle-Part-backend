using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpPost("staff/register")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto, CancellationToken cancellationToken)
    {
        await adminService.RegisterStaffAsync(dto, cancellationToken);
        return Ok("Staff registered successfully.");
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetAllStaff(CancellationToken cancellationToken)
    {
        var staff = await adminService.GetAllUsersAsync(cancellationToken);
        return Ok(staff);
    }

    [HttpDelete("staff/{id}")]
    public async Task<IActionResult> DeleteStaff(Guid id, CancellationToken cancellationToken)
    {
        await adminService.DeleteUserAsync(id, cancellationToken);
        return Ok("Staff deleted successfully.");
    }

    [HttpPut("staff/role")]
    public async Task<IActionResult> UpdateStaffRole([FromBody] UpdateStaffRoleDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffRoleAsync(dto, cancellationToken);
        return Ok("Role updated successfully.");
    }

    [HttpGet("parts")]
    public async Task<IActionResult> GetAllParts(CancellationToken cancellationToken) => Ok(await adminService.GetAllPartsAsync(cancellationToken));

    [HttpPost("parts")]
    public async Task<IActionResult> AddPart([FromBody] AddPartDto dto, CancellationToken cancellationToken) => Ok(await adminService.AddPartAsync(dto, cancellationToken));

    [HttpPut("parts/{id}")]
    public async Task<IActionResult> UpdatePart(Guid id, [FromBody] UpdatePartDto dto, CancellationToken cancellationToken) => Ok(await adminService.UpdatePartAsync(id, dto, cancellationToken));

    [HttpDelete("parts/{id}")]
    public async Task<IActionResult> DeletePart(Guid id, CancellationToken cancellationToken)
    {
        await adminService.DeletePartAsync(id, cancellationToken);
        return Ok("Part deleted successfully.");
    }

    [HttpPost("parts/{id}/purchase")]
    public async Task<IActionResult> PurchasePart(Guid id, int quantity, [FromBody] PurchasePartDto dto, CancellationToken cancellationToken)
    {
        await adminService.PurchasePartAsync(id, quantity, dto, cancellationToken);
        return Ok("Stock updated.");
    }

    [HttpGet("financial-reports/{reportType}")]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(string reportType, CancellationToken cancellationToken) => Ok(await adminService.GetFinancialReportAsync(reportType, cancellationToken));
}
