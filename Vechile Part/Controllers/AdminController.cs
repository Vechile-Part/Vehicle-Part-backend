using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("financial-reports/{reportType}")]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(string reportType, CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetFinancialReportAsync(reportType, cancellationToken));
    }
}
