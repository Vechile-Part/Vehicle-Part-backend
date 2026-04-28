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
        try
        {
            return Ok(await customerService.GetVehicleHealthAIAsync(vehicleId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{customerId:guid}/appointments")]
    public async Task<ActionResult<Guid>> BookAppointment(Guid customerId, [FromBody] AppointmentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var id = await customerService.BookAppointmentAsync(customerId, dto with { CustomerId = customerId }, cancellationToken);
            return Created($"/api/customers/{customerId}/appointments/{id}", id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{customerId:guid}/part-requests")]
    public async Task<ActionResult<Guid>> RequestPart(Guid customerId, [FromBody] PartRequestDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var id = await customerService.RequestPartAsync(customerId, dto with { CustomerId = customerId }, cancellationToken);
            return Created($"/api/customers/{customerId}/part-requests/{id}", id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{customerId:guid}/service-reviews")]
    public async Task<ActionResult<Guid>> SubmitServiceReview(Guid customerId, [FromBody] ServiceReviewDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var id = await customerService.SubmitServiceReviewAsync(customerId, dto with { CustomerId = customerId }, cancellationToken);
            return Created($"/api/customers/{customerId}/service-reviews/{id}", id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{customerId:guid}/appointments")]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponseDto>>> GetAppointments(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await customerService.GetAppointmentsByCustomerAsync(customerId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{customerId:guid}/part-requests")]
    public async Task<ActionResult<IReadOnlyList<PartRequestResponseDto>>> GetPartRequests(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await customerService.GetPartRequestsByCustomerAsync(customerId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{customerId:guid}/reviews")]
    public async Task<ActionResult<IReadOnlyList<ServiceReviewResponseDto>>> GetReviews(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await customerService.GetReviewsByCustomerAsync(customerId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{customerId:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<PurchaseHistoryDto>>> GetPurchaseHistory(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await customerService.GetPurchaseHistoryAsync(customerId, cancellationToken));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
