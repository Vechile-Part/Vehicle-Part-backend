using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/staff")]
[Authorize(Roles = "Staff,Admin")]
public class StaffController(IStaffService staffService, IAdminService adminService) : ControllerBase
{
    [HttpGet("parts")]
    public async Task<IActionResult> GetParts(CancellationToken cancellationToken) =>
        Ok(await adminService.GetAllPartsAsync(cancellationToken));

    [HttpPost("customers")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegistrationDto dto, CancellationToken cancellationToken)
    {
        var customerId = await staffService.RegisterCustomerWithVehicleAsync(dto, cancellationToken);
        return Ok(new
        {
            CustomerId = customerId,
            Message = "Customer registered. If email delivery is configured, they were sent a link to set their password.",
        });
    }

    [HttpPost("sales-invoices")]
    public async Task<ActionResult<SalesInvoiceResponseDto>> CreateSalesInvoice(
        [FromBody] SalesInvoiceCreateDto dto,
        CancellationToken cancellationToken)
    {
        var result = await staffService.CreateSalesInvoiceAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("sales-invoices")]
    public async Task<IActionResult> ListSalesInvoices(CancellationToken cancellationToken)
        => Ok(await staffService.ListSalesInvoicesAsync(cancellationToken));

    [HttpGet("sales-invoices/{invoiceId:guid}")]
    public async Task<IActionResult> GetSalesInvoice(Guid invoiceId, CancellationToken cancellationToken)
    {
        var result = await staffService.GetSalesInvoiceAsync(invoiceId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("customers/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerDetails(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await staffService.GetCustomerDetailsAsync(customerId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("customer-reports")]
    public async Task<ActionResult<CustomerReportDto>> GetCustomerReport(CancellationToken cancellationToken)
        => Ok(await staffService.GetCustomerReportAsync(cancellationToken));

    [HttpGet("customers/search")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? vehicleNumber,
        [FromQuery] string? phone,
        [FromQuery] string? fullName,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        var results = await staffService.SearchCustomersAsync(
            new CustomerSearchDto(vehicleNumber, phone, fullName, customerId),
            cancellationToken);
        return Ok(results);
    }

    [HttpPost("sales-invoices/{invoiceId:guid}/send-email")]
    public async Task<IActionResult> SendInvoiceEmail(Guid invoiceId, CancellationToken cancellationToken)
    {
        await staffService.SendInvoiceEmailAsync(invoiceId, cancellationToken);
        return Ok(new { Message = "Invoice email sent successfully." });
    }
}
