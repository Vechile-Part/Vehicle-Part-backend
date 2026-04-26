using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VechilePart.Domain.Entities;
using VechilePart.Infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase {
    private readonly AppDbContext _context; 

    public CustomersController(AppDbContext context) { _context = context; }

    [HttpGet]
    public async Task<IActionResult> GetCustomers() {
        return Ok(await _context.Customers.Include(c => c.Vehicles).ToListAsync());
    }
}