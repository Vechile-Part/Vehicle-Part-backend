using Microsoft.AspNetCore.Mvc;
using VechilePart.Application.DTOs;
using VechilePart.Application.Interfaces;

namespace VechilePart.Controllers;

[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerFeatureService _customerFeatureService;

    public CustomersController(ICustomerFeatureService customerFeatureService)
    {
        _customerFeatureService = customerFeatureService;
    }

    // Feature 13 - Book Appointment
    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentDto dto)
    {
        var result = await _customerFeatureService.BookAppointmentAsync(dto);
        return Ok(result);
    }

    // Feature 13 - Request Unavailable Part
    [HttpPost("part-requests")]
    public async Task<IActionResult> RequestPart([FromBody] RequestPartDto dto)
    {
        var result = await _customerFeatureService.RequestPartAsync(dto);
        return Ok(result);
    }

    // Feature 13 - Submit Service Review
    [HttpPost("reviews")]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewDto dto)
    {
        var result = await _customerFeatureService.SubmitReviewAsync(dto);
        return Ok(result);
    }

    // Feature 13 - Get Appointments
    [HttpGet("{customerId}/appointments")]
    public async Task<IActionResult> GetAppointments(Guid customerId)
    {
        var result = await _customerFeatureService.GetAppointmentsByCustomerAsync(customerId);
        return Ok(result);
    }

    // Feature 13 - Get Part Requests
    [HttpGet("{customerId}/part-requests")]
    public async Task<IActionResult> GetPartRequests(Guid customerId)
    {
        var result = await _customerFeatureService.GetPartRequestsByCustomerAsync(customerId);
        return Ok(result);
    }

    // Feature 13 - Get Reviews
    [HttpGet("{customerId}/reviews")]
    public async Task<IActionResult> GetReviews(Guid customerId)
    {
        var result = await _customerFeatureService.GetReviewsByCustomerAsync(customerId);
        return Ok(result);
    }

    // Feature 14 - Purchase/Service History
    [HttpGet("{customerId}/history")]
    public async Task<IActionResult> GetPurchaseHistory(Guid customerId)
    {
        var result = await _customerFeatureService.GetPurchaseHistoryAsync(customerId);
        return Ok(result);
    }
}