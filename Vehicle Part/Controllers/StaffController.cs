using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;
namespace Vehicle_Part.Controllers;
[ApiController]
[Route("api/staff")]
public class StaffController(IAdminService adminService, IStaffService staffService) : ControllerBase
{
    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RegisterStaff([FromBody] StaffRegistrationDto dto, CancellationToken cancellationToken)
    {
        await adminService.RegisterStaffAsync(dto, cancellationToken);
        return Ok("Staff registered successfully.");
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllStaff(CancellationToken cancellationToken) => Ok(await adminService.GetAllUsersAsync(cancellationToken));
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(Guid id, CancellationToken cancellationToken)
    {
        await adminService.DeleteUserAsync(id, cancellationToken);
        return Ok("Staff deleted successfully.");
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("role")]
    public async Task<IActionResult> UpdateStaffRole([FromBody] UpdateStaffRoleDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffRoleAsync(dto, cancellationToken);
        return Ok("Role updated successfully.");
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("details")]
    public async Task<IActionResult> UpdateStaffDetails([FromBody] UpdateStaffDetailsDto dto, CancellationToken cancellationToken)
    {
        await adminService.UpdateStaffDetailsAsync(dto, cancellationToken);
        return Ok("Staff details updated successfully.");
    }
    [HttpPost("customers")]
    public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegistrationDto dto, CancellationToken cancellationToken)
    {
        var customerId = await staffService.RegisterCustomerWithVehicleAsync(dto, cancellationToken);
        return Ok(new { CustomerId = customerId, Message = "Customer registered successfully." });
    }

    [HttpPost("sales-invoices")]
    public async Task<ActionResult<Guid>> CreateSalesInvoice([FromBody] SalesInvoiceCreateDto dto, CancellationToken cancellationToken) => Ok(await staffService.CreateSalesInvoiceAsync(dto, cancellationToken));

    [HttpGet("customers/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerDetails(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await staffService.GetCustomerDetailsAsync(customerId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("customer-reports")]
    public async Task<ActionResult<CustomerReportDto>> GetCustomerReport(CancellationToken cancellationToken) => Ok(await staffService.GetCustomerReportAsync(cancellationToken));

    [HttpGet("customers/search")]
    public async Task<IActionResult> SearchCustomers([FromQuery] string? vehicleNumber, [FromQuery] string? phone, [FromQuery] string? fullName, CancellationToken cancellationToken) => Ok(await staffService.SearchCustomersAsync(new CustomerSearchDto(vehicleNumber, phone, fullName), cancellationToken));

    [HttpPost("sales-invoices/{invoiceId:guid}/send-email")]
    public async Task<IActionResult> SendInvoiceEmail(Guid invoiceId, CancellationToken cancellationToken)
    {
        await staffService.SendInvoiceEmailAsync(invoiceId, cancellationToken);
        return Ok(new { Message = "Invoice email sent successfully." });
    }
}
