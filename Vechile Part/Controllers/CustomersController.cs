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
}
