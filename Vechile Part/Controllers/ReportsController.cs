using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(IAdminService adminService) : ControllerBase
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

    [HttpGet("customers")]
    public async Task<ActionResult<CustomerReportDto>> GetCustomerReport(CancellationToken ct)
    {
        return Ok(await staffService.GetCustomerReportAsync(ct));
    }
}
