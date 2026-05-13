using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/customer-history")]
[Authorize(Roles = "Customer")]
public class CustomerHistoryController(ICustomerHistoryService customerHistoryService) : ControllerBase
{
    private const string CustomerIdClaimType = "CustomerId";

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetCustomerHistory(Guid customerId, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedCustomerId(out var tokenCustomerId))
            return Unauthorized();

        if (tokenCustomerId != customerId)
            return Forbid();

        var result = await customerHistoryService.GetCustomerHistoryAsync(customerId, cancellationToken);

        if (result is null)
            return NotFound("Customer not found.");

        return Ok(result);
    }

    private bool TryGetAuthenticatedCustomerId(out Guid customerId)
    {
        customerId = Guid.Empty;
        var raw = User.FindFirst(CustomerIdClaimType)?.Value;
        return raw is not null && Guid.TryParse(raw, out customerId);
    }
}
