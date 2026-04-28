using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    // ADD these inside the existing AdminController class

// Feature 2: Staff Registration & Role Management
    [HttpPost("staff/register")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto, CancellationToken cancellationToken)
    {
        await adminService.RegisterStaffAsync(dto, cancellationToken);
        return Ok("Staff registered successfully.");
    }

    [HttpPut("staff/role")]
    public async Task<IActionResult> UpdateStaffRole([FromBody] UpdateStaffRoleDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffRoleAsync(dto, cancellationToken);
        return Ok("Staff role updated successfully.");
    }

// Feature 3: Parts Management
    [HttpGet("parts")]
    public async Task<IActionResult> GetAllParts(CancellationToken cancellationToken)
    {
        var parts = await adminService.GetAllPartsAsync(cancellationToken);
        return Ok(parts);
    }

    [HttpPost("parts")]
    public async Task<IActionResult> AddPart([FromBody] AddPartDto dto, CancellationToken cancellationToken)
    {
        var part = await adminService.AddPartAsync(dto, cancellationToken);
        return Ok(part);
    }

    [HttpPut("parts/{id}")]
    public async Task<IActionResult> UpdatePart(Guid id, [FromBody] UpdatePartDto dto, CancellationToken cancellationToken)
    {
        var part = await adminService.UpdatePartAsync(id, dto, cancellationToken);
        return Ok(part);
    }

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
        return Ok("Part purchased and stock updated.");
    }
    [HttpGet("financial-reports/{reportType}")]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(string reportType, CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetFinancialReportAsync(reportType, cancellationToken));
    }
}
