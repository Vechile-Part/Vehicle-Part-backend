using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs.PurchaseInvoice;
using VehiclePart.Application.Interfaces;

namespace VehiclePart.Controllers;

[ApiController]
[Route("api/purchase-invoices")]
public class PurchaseInvoiceController(IPurchaseInvoiceService purchaseInvoiceService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto, CancellationToken cancellationToken)
    {
        var result = await purchaseInvoiceService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await purchaseInvoiceService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await purchaseInvoiceService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound("Purchase invoice not found.");

        return Ok(result);
    }
}