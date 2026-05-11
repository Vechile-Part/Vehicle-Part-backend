using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;


[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/vendors")]
public class VendorController(IVendorService vendorService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await vendorService.GetAllVendorsAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await vendorService.GetVendorByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorDto dto, CancellationToken cancellationToken)
    {
        var created = await vendorService.CreateVendorAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVendorDto dto, CancellationToken cancellationToken)
        => Ok(await vendorService.UpdateVendorAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await vendorService.DeleteVendorAsync(id, cancellationToken);
        return Ok(new { Message = "Vendor deleted successfully." });
    }
}
