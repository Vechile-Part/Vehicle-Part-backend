using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/parts")]
public class PartsController(IAdminService adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllParts(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        if (page.HasValue && pageSize.HasValue)
        {
            return Ok(await adminService.GetPagedPartsAsync(page.Value, pageSize.Value, search, cancellationToken));
        }
        return Ok(await adminService.GetAllPartsAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> AddPart([FromBody] AddPartDto dto, CancellationToken cancellationToken) =>
        Ok(await adminService.AddPartAsync(dto, cancellationToken));

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePart(Guid id, [FromBody] UpdatePartDto dto, CancellationToken cancellationToken) =>
        Ok(await adminService.UpdatePartAsync(id, dto, cancellationToken));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePart(Guid id, CancellationToken cancellationToken)
    {
        await adminService.DeletePartAsync(id, cancellationToken);
        return Ok("Part deleted successfully.");
    }

    [HttpPost("{id}/purchase")]
    public async Task<IActionResult> PurchasePart(Guid id, int quantity, [FromBody] PurchasePartDto dto, CancellationToken cancellationToken)
    {
        await adminService.PurchasePartAsync(id, quantity, dto, cancellationToken);
        return Ok("Stock updated.");
    }
}
