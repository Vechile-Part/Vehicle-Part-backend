using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.Interfaces;
using VehiclePart.Application.DTOs;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/staff")]
public class StaffController(IStaffService staffService) : ControllerBase
{
    [HttpPost("customers")]
    public async Task<IActionResult> RegisterCustomer(
        [FromBody] CustomerRegistrationDto dto,
        CancellationToken cancellationToken)
    {
        var customerId = await staffService.RegisterCustomerWithVehicleAsync(dto, cancellationToken);

        return Ok(new
        {
            CustomerId = customerId,
            Message = "Customer registered successfully."
        });
    }

    [HttpPost("sales-invoices")]
    public async Task<ActionResult<Guid>> CreateSalesInvoice(
        [FromBody] SalesInvoiceCreateDto dto,
        CancellationToken cancellationToken)
    {
        return Ok(await staffService.CreateSalesInvoiceAsync(dto, cancellationToken));
    }

    [HttpGet("customers/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerDetails(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var result = await staffService.GetCustomerDetailsAsync(customerId, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("customer-reports")]
    public async Task<ActionResult<CustomerReportDto>> GetCustomerReport(
        CancellationToken cancellationToken)
    {
        return Ok(await staffService.GetCustomerReportAsync(cancellationToken));
    }

    [HttpGet("customers/search")]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string? vehicleNumber,
        [FromQuery] string? phone,
        [FromQuery] string? fullName,
        CancellationToken cancellationToken)
    {
        return Ok(await staffService.SearchCustomersAsync(
            new CustomerSearchDto(vehicleNumber, phone, fullName),
            cancellationToken));
    }

    [HttpPost("sales-invoices/{invoiceId:guid}/send-email")]
    public async Task<IActionResult> SendInvoiceEmail(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        await staffService.SendInvoiceEmailAsync(invoiceId, cancellationToken);

        return Ok(new
        {
            Message = "Invoice email sent successfully."
        });
    }
}