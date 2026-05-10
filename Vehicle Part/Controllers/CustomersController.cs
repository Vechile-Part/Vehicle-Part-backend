using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController(ICustomerService customerService, IStaffService staffService) : ControllerBase
{
    private const string CustomerIdClaimType = "CustomerId";

    [Authorize(Roles = "Staff")]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterByStaff([FromBody] CustomerRegistrationDto dto, CancellationToken cancellationToken)
    {
        var customerId = await staffService.RegisterCustomerWithVehicleAsync(dto, cancellationToken);
        return Ok(new { CustomerId = customerId, Message = "Customer registered successfully." });
    }

    [AllowAnonymous]
    [HttpPost("self-register")]
    public async Task<ActionResult<Guid>> SelfRegister([FromBody] CustomerSelfRegistrationDto dto, CancellationToken cancellationToken)
    {
        return Ok(await customerService.SelfRegisterAsync(dto, cancellationToken));
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{customerId:guid}/vehicles")]
    public async Task<IActionResult> AddVehicle(Guid customerId, [FromBody] VehicleDto dto, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        await customerService.AddVehicleAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("{customerId:guid}/profile")]
    public async Task<IActionResult> GetProfile(Guid customerId, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        var profile = await customerService.GetProfileAsync(customerId, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("{customerId:guid}/profile")]
    public async Task<IActionResult> UpdateProfile(Guid customerId, [FromBody] CustomerProfileDto dto, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        await customerService.UpdateProfileAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("{customerId:guid}/vehicles")]
    public async Task<IActionResult> GetVehicles(Guid customerId, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        return Ok(await customerService.GetCustomerVehiclesAsync(customerId, cancellationToken));
    }

    [Authorize(Roles = "Customer")]
    [HttpPut("{customerId:guid}/vehicles/{vehicleId:guid}")]
    public async Task<IActionResult> UpdateVehicle(Guid customerId, Guid vehicleId, [FromBody] VehicleDto dto, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        await customerService.UpdateVehicleAsync(customerId, dto with { Id = vehicleId }, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("vehicles/{vehicleId:guid}/ai-health")]
    public async Task<IActionResult> GetVehicleHealth(Guid vehicleId, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedCustomerId(out var customerId))
            return Unauthorized();

        return Ok(await customerService.GetVehicleHealthAIAsync(vehicleId, customerId, cancellationToken));
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{customerId:guid}/appointments")]
    public async Task<IActionResult> BookAppointment(Guid customerId, [FromBody] BookAppointmentDto dto, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        await customerService.BookAppointmentAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("{customerId:guid}/appointments")]
    public async Task<IActionResult> GetAppointments(Guid customerId, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        return Ok(await customerService.GetAppointmentsAsync(customerId, cancellationToken));
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{customerId:guid}/part-requests")]
    public async Task<IActionResult> RequestPart(Guid customerId, [FromBody] PartRequestDto dto, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        await customerService.RequestPartAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = "Customer")]
    [HttpPost("{customerId:guid}/reviews")]
    public async Task<IActionResult> ReviewService(Guid customerId, [FromBody] ServiceReviewDto dto, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        await customerService.ReviewServiceAsync(customerId, dto, cancellationToken);
        return Ok();
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("{customerId:guid}/history/purchases")]
    public async Task<IActionResult> GetPurchaseHistory(Guid customerId, CancellationToken cancellationToken)
    {
        var forbid = EnsureCustomerOwnsRoute(customerId);
        if (forbid is not null)
            return forbid;

        return Ok(await customerService.GetPurchaseHistoryAsync(customerId, cancellationToken));
    }

    private IActionResult? EnsureCustomerOwnsRoute(Guid routeCustomerId)
    {
        if (!TryGetAuthenticatedCustomerId(out var tokenCustomerId))
            return Unauthorized();

        return tokenCustomerId != routeCustomerId ? Forbid() : null;
    }

    private bool TryGetAuthenticatedCustomerId(out Guid customerId)
    {
        customerId = Guid.Empty;
        var raw = User.FindFirst(CustomerIdClaimType)?.Value;
        return raw is not null && Guid.TryParse(raw, out customerId);
    }
}
