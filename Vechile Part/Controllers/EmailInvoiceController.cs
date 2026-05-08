using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailInvoiceController(IEmailInvoiceService emailInvoiceService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> SendInvoiceEmail([FromBody] EmailInvoiceDto dto)
    {
        var result = await emailInvoiceService.SendInvoiceEmailAsync(dto);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Failed to send invoice email"
            });
        }

        return Ok(new
        {
            message = "Invoice email sent successfully"
        });
    }
}