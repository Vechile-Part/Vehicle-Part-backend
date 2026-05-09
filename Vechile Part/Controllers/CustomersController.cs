using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("{customerId:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetProfileAsync(customerId, cancellationToken));
    }
}