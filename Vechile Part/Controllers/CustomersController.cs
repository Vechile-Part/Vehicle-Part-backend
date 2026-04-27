using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.Interfaces;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase {
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService) {
        _customerService = customerService;
    }

    [HttpGet]
    public IActionResult GetStatus() {
        return Ok("Customer API is running");
    }
}