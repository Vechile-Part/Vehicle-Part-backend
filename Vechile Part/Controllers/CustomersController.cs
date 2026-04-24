using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpPost("self-register")]
    public async Task<ActionResult<Guid>> SelfRegister([FromBody] CustomerSelfRegistrationDto dto, CancellationToken cancellationToken)
    {
        return Ok(await customerService.SelfRegisterAsync(dto, cancellationToken));
    }

    [HttpPost("{customerId:guid}/vehicles")]
    public async Task<IActionResult> AddVehicle(Guid customerId, [FromBody] VehicleDto dto, CancellationToken cancellationToken)
    {
        await customerService.AddVehicleAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment([FromBody] AppointmentDto dto, CancellationToken cancellationToken)
    {
        await customerService.BookAppointmentAsync(dto, cancellationToken);
        return Ok();
    }

    [HttpPost("part-requests")]
    public async Task<IActionResult> RequestPart([FromBody] PartRequestDto dto, CancellationToken cancellationToken)
    {
        await customerService.RequestPartAsync(dto, cancellationToken);
        return Ok();
    }

    [HttpPost("reviews")]
    public async Task<IActionResult> AddReview([FromBody] ServiceReviewDto dto, CancellationToken cancellationToken)
    {
        await customerService.AddServiceReviewAsync(dto, cancellationToken);
        return Ok();
    }

    [HttpGet("{customerId:guid}/history")]
    public async Task<IActionResult> GetHistory(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetPurchaseAndServiceHistoryAsync(customerId, cancellationToken));
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
