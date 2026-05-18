using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;
using VehiclePart.Infrastructure.Data;

namespace Vehicle_Part.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService, AppDbContext dbContext) : ControllerBase
{
    [HttpPost("staff/register")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto, CancellationToken cancellationToken)
    {
        await adminService.RegisterStaffAsync(dto, cancellationToken);
        return Ok("Staff registered successfully.");
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken) =>
        Ok(await adminService.GetAdminDashboardAsync(cancellationToken));

    [HttpGet("staff")]
    public async Task<IActionResult> GetAllStaff(CancellationToken cancellationToken) => Ok(await adminService.GetAllUsersAsync(cancellationToken));

    [HttpGet("customer-accounts")]
    public async Task<IActionResult> GetCustomerAccounts(CancellationToken cancellationToken) =>
        Ok(await adminService.GetCustomerAccountsAsync(cancellationToken));

    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken) =>
        Ok(await adminService.GetCustomerAccountsAsync(cancellationToken));

    [HttpDelete("staff/{id}")]
    public async Task<IActionResult> RemoveStaffFromRole(Guid id, CancellationToken cancellationToken)
    {
        await adminService.DemoteStaffToCustomerAsync(id, cancellationToken);
        return Ok("Staff access removed. User is now a customer.");
    }

    [HttpPut("staff/role")]
    public async Task<IActionResult> UpdateStaffRole([FromBody] UpdateStaffRoleDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffRoleAsync(dto, cancellationToken);
        return Ok("Role updated successfully.");
    }

    [HttpPut("staff/details")]
    public async Task<IActionResult> UpdateStaffDetails([FromBody] UpdateStaffDetailsDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffDetailsAsync(dto, cancellationToken);
        return Ok("Staff details updated successfully.");
    }

    [HttpGet("parts")]
    public async Task<IActionResult> GetAllParts(CancellationToken cancellationToken) => Ok(await adminService.GetAllPartsAsync(cancellationToken));

    [HttpPost("parts/repair-vendor-links")]
    public async Task<IActionResult> RepairPartVendorLinks(CancellationToken cancellationToken)
    {
        await PartVendorBootstrap.RepairAsync(dbContext, assignSoleVendorToOrphans: true);
        return Ok(await adminService.GetAllPartsAsync(cancellationToken));
    }

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

    /// <summary>Chart and table data for the financial reports UI (daily = last 7 days, monthly = same window with month context, yearly = last 7 months).</summary>
    [HttpGet("financial-dashboard/{period}")]
    public async Task<ActionResult<FinancialDashboardDto>> GetFinancialDashboard(string period, CancellationToken cancellationToken) =>
        Ok(await adminService.GetFinancialDashboardAsync(period, cancellationToken));

    [HttpGet("part-requests")]
    public async Task<IActionResult> GetPartRequests(CancellationToken cancellationToken) =>
        Ok(await adminService.GetPartRequestsAsync(cancellationToken));

    [HttpPut("part-requests/{id:guid}/status")]
    public async Task<IActionResult> UpdatePartRequestStatus(
        Guid id,
        [FromBody] UpdatePartRequestStatusDto dto,
        CancellationToken cancellationToken)
    {
        await adminService.UpdatePartRequestStatusAsync(id, dto, cancellationToken);
        return Ok("Part request status updated.");
    }
}
