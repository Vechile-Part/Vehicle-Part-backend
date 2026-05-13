using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Roles = "Admin")]
public class NotificationsController(IAdminService adminService) : ControllerBase
{
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockNotifications(CancellationToken cancellationToken)
    {
        var parts = await adminService.GetLowStockPartsAsync(10, cancellationToken);
        return Ok(parts);
    }

    /// <summary>
    /// Sales invoices with pending credit older than one month (same rule as automated customer reminders).
    /// </summary>
    [HttpGet("overdue-credits")]
    public async Task<IActionResult> GetOverdueCredits(CancellationToken cancellationToken)
    {
        var rows = await adminService.GetOverdueCreditInvoicesAsync(1, cancellationToken);
        return Ok(rows);
    }
}
