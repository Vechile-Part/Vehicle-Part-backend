using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Admin")]
public class NotificationsController(IAdminService adminService, INotificationJobRunner notificationJobRunner) : ControllerBase
{
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockNotifications(CancellationToken cancellationToken)
    {
        var parts = await adminService.GetLowStockPartsAsync(10, cancellationToken);
        return Ok(parts);
    }

    [HttpGet("overdue-credits")]
    public async Task<IActionResult> GetOverdueCredits(CancellationToken cancellationToken)
    {
        var rows = await adminService.GetOverdueCreditInvoicesAsync(1, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("admin-summary")]
    public async Task<IActionResult> GetAdminSummary(CancellationToken cancellationToken)
    {
        var lowStock = await adminService.GetLowStockPartsAsync(10, cancellationToken);
        var overdue = await adminService.GetOverdueCreditInvoicesAsync(1, cancellationToken);

        return Ok(new
        {
            lowStockCount = lowStock.Count,
            overdueCreditCount = overdue.Count,
            lowStockItems = lowStock.Take(8)
        });
    }

    [HttpPost("run-jobs")]
    public async Task<IActionResult> RunNotificationJobs(
        [FromQuery] bool force = false,
        CancellationToken cancellationToken = default)
    {
        var result = await notificationJobRunner.RunAsync(force, cancellationToken);
        return Ok(result);
    }
}
