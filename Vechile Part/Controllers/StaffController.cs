using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VechilePart.Domain.Entities;
using VechilePart.Application.DTOs;
using VechilePart.Infrastructure.Data; 

[ApiController]
[Route("api/[controller]")]
public class StaffController : ControllerBase {
    private readonly AppDbContext _context; 

    public StaffController(AppDbContext context) { _context = context; }

    [HttpPost("register-customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegistrationDto dto) {
        var customer = new Customer { FullName = dto.Name, Email = dto.Email, Phone = dto.Phone };
        var vehicle = new Vehicle { PlateNumber = dto.VehiclePlateNumber, Model = dto.VehicleModel, Customer = customer };
        _context.Customers.Add(customer);
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return Ok(new { customer, vehicle });
    }
}