using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/customer-history")]
public class CustomerHistoryController(ICustomerHistoryService customerHistoryService) : ControllerBase
{
    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetCustomerHistory(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await customerHistoryService.GetCustomerHistoryAsync(customerId, cancellationToken);

        if (result is null)
            return NotFound("Customer not found.");

        return Ok(result);
    }
}