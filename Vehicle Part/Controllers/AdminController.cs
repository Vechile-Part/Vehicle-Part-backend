using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("financial-reports/{reportType}")]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(string reportType, CancellationToken cancellationToken) 
        => Ok(await adminService.GetFinancialReportAsync(reportType, cancellationToken));
}