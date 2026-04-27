using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase {
    private readonly ICustomerService _customerService;

    public StaffController(ICustomerService customerService) {
        _customerService = customerService;
    }

    // Task 6: Register Customer and Vehicle
    [HttpPost("register-customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegistrationDto dto) {
        await _customerService.RegisterCustomerAsync(dto);
        return Ok("Customer and Vehicle registered successfully");
    }
}