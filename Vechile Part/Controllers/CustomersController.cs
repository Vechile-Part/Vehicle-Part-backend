using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VechilePart.Application.DTOs;
using VechilePart.Application.Interfaces;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpPost("self-register")]
    public async Task<ActionResult<Guid>> SelfRegister([FromBody] CustomerSelfRegistrationDto dto, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await customerService.SelfRegisterAsync(dto, cancellationToken));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Registration is temporarily unavailable. Please check database connection and try again.",
                detail = ex.Message
            });
        }
    }

    [HttpPost("{customerId:guid}/vehicles")]
    public async Task<IActionResult> AddVehicle(Guid customerId, [FromBody] VehicleDto dto, CancellationToken cancellationToken)
    {
        await customerService.AddVehicleAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [HttpGet("{customerId:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetProfileAsync(customerId, cancellationToken));
    }

    [HttpPut("{customerId:guid}/profile")]
    public async Task<IActionResult> UpdateProfile(Guid customerId, [FromBody] CustomerProfileDto dto, CancellationToken cancellationToken)
    {
        await customerService.UpdateProfileAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [HttpGet("{customerId:guid}/vehicles")]
    public async Task<IActionResult> GetVehicles(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetCustomerVehiclesAsync(customerId, cancellationToken));
    }

    [HttpPut("{customerId:guid}/vehicles/{vehicleId:guid}")]
    public async Task<IActionResult> UpdateVehicle(Guid customerId, Guid vehicleId, [FromBody] VehicleDto dto, CancellationToken cancellationToken)
    {
        await customerService.UpdateVehicleAsync(customerId, dto with { Id = vehicleId }, cancellationToken);
        return Ok();
    }

    [HttpGet("vehicles/{vehicleId:guid}/ai-health")]
    public async Task<IActionResult> GetVehicleHealth(Guid vehicleId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetVehicleHealthAIAsync(vehicleId, cancellationToken));
    }
}
