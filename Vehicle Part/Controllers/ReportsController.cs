using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Admin")]
public class ReportsController(IAdminService adminService, IStaffService staffService) : ControllerBase
{
    [HttpGet("financial")]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport([FromQuery] string type = "Monthly", CancellationToken ct = default)
    {
        return Ok(await adminService.GetFinancialReportAsync(type, ct));
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock(CancellationToken ct)
    {
        return Ok(await adminService.GetLowStockPartsAsync(10, ct));
    }

    [HttpGet("customers/top-spenders")]
    public async Task<IActionResult> GetTopSpenders(CancellationToken ct)
    {
        var report = await staffService.GetCustomerReportAsync(ct);
        return Ok(report.HighSpenderRows);
    }
}
