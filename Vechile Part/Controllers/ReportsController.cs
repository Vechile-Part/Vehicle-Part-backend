using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

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

    [HttpGet("customers/top-spenders")]
    public async Task<IActionResult> GetTopSpenders(CancellationToken ct)
    {
        var report = new[]
        {
            new { Name = "John Smith", TotalSales = 4500.00m, CustomerId = Guid.NewGuid() },
            new { Name = "Sarah Johnson", TotalSales = 3200.50m, CustomerId = Guid.NewGuid() },
            new { Name = "Mike Wilson", TotalSales = 1200.00m, CustomerId = Guid.NewGuid() }
        };
        return Ok(report);
    }
}
