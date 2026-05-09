using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(ICustomerService customerService, IStaffService staffService) : ControllerBase
{
    [Authorize(Roles = "Staff")]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterByStaff([FromBody] CustomerRegistrationDto dto, CancellationToken cancellationToken)
    {
        var customerId = await staffService.RegisterCustomerWithVehicleAsync(dto, cancellationToken);
        return Ok(new { CustomerId = customerId, Message = "Customer registered successfully." });
    }

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


    [HttpPost("{customerId:guid}/appointments")]
    public async Task<IActionResult> BookAppointment(Guid customerId, [FromBody] BookAppointmentDto dto, CancellationToken cancellationToken)
    {
        await customerService.BookAppointmentAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [HttpPost("{customerId:guid}/part-requests")]
    public async Task<IActionResult> RequestPart(Guid customerId, [FromBody] PartRequestDto dto, CancellationToken cancellationToken)
    {
        await customerService.RequestPartAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [HttpPost("{customerId:guid}/reviews")]
    public async Task<IActionResult> ReviewService(Guid customerId, [FromBody] ServiceReviewDto dto, CancellationToken cancellationToken)
    {
        await customerService.ReviewServiceAsync(customerId, dto, cancellationToken);
        return Ok();
    }


    [HttpGet("{customerId:guid}/history/purchases")]
    public async Task<IActionResult> GetPurchaseHistory(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetPurchaseHistoryAsync(customerId, cancellationToken));
    }
}  