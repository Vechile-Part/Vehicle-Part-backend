using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController(IAdminService adminService) : ControllerBase
{
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockNotifications(CancellationToken cancellationToken)
    {
        var parts = await adminService.GetLowStockPartsAsync(10, cancellationToken);
        return Ok(parts);
    }
}
