using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.Interfaces;
using VechilePart.Application.DTOs;

namespace Vechile_Part.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("financial-reports/{reportType}")]
    public async Task<ActionResult<FinancialReportDto>> GetFinancialReport(string reportType, CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetFinancialReportAsync(reportType, cancellationToken));
    }

    [HttpPost("staff")]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto, CancellationToken cancellationToken)
    {
        await adminService.RegisterStaffAsync(dto, cancellationToken);
        return Ok();
    }

    [HttpPost("parts")]
    public async Task<ActionResult<PartDto>> UpsertPart([FromBody] PartDto dto, CancellationToken cancellationToken)
    {
        return Ok(await adminService.UpsertPartAsync(dto, cancellationToken));
    }

    [HttpDelete("parts/{partId:guid}")]
    public async Task<IActionResult> DeletePart(Guid partId, CancellationToken cancellationToken)
    {
        await adminService.DeletePartAsync(partId, cancellationToken);
        return NoContent();
    }

    [HttpPost("purchase-invoices")]
    public async Task<ActionResult<PurchaseInvoiceDto>> CreatePurchaseInvoice([FromBody] PurchaseInvoiceDto dto, CancellationToken cancellationToken)
    {
        return Ok(await adminService.CreatePurchaseInvoiceAsync(dto, cancellationToken));
    }

    [HttpPost("vendors")]
    public async Task<ActionResult<VendorDto>> UpsertVendor([FromBody] VendorDto dto, CancellationToken cancellationToken)
    {
        return Ok(await adminService.UpsertVendorAsync(dto, cancellationToken));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IReadOnlyList<PartDto>>> GetLowStockParts([FromQuery] int threshold = 10, CancellationToken cancellationToken = default)
    {
        return Ok(await adminService.GetLowStockPartsAsync(threshold, cancellationToken));
    }
}
