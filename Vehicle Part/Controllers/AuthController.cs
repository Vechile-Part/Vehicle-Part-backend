using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehiclePart.Application.DTOs;
using VehiclePart.Application.Interfaces;

namespace Vehicle_Part.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    IAuthService authService,
    ICustomerInviteService customerInviteService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await authService.LoginStaffAsync(dto?.Email, dto?.Password ?? string.Empty, cancellationToken);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Error);

        return Ok(new { token = result.Value!.Token, role = result.Value.Role });
    }

    [HttpPost("customer/login")]
    public async Task<IActionResult> CustomerLogin([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var result = await authService.LoginCustomerAsync(dto.Email, dto.Password, cancellationToken);
        if (!result.IsSuccess)
            return Unauthorized(result.Error);

        return Ok(new
        {
            token = result.Value!.Token,
            role = result.Value.Role,
            customerId = result.Value.CustomerId
        });
    }

    [AllowAnonymous]
    [HttpPost("customer/complete-invite-password")]
    public async Task<IActionResult> CompleteCustomerPasswordInvite(
        [FromBody] CompleteCustomerPasswordInviteDto dto,
        CancellationToken cancellationToken)
    {
        var (ok, error) = await customerInviteService.TryCompletePasswordInviteAsync(
            dto.Token,
            dto.NewPassword,
            cancellationToken);

        if (!ok)
            return BadRequest(new { message = error ?? "Request could not be completed." });

        return Ok(new { message = "Password saved. You can sign in now." });
    }

    [AllowAnonymous]
    [HttpPost("staff/complete-invite-password")]
    public async Task<IActionResult> CompleteStaffPasswordInvite(
        [FromBody] CompleteCustomerPasswordInviteDto dto,
        CancellationToken cancellationToken)
    {
        var (ok, error) = await customerInviteService.TryCompleteStaffPasswordInviteAsync(
            dto.Token,
            dto.NewPassword,
            cancellationToken);

        if (!ok)
            return BadRequest(new { message = error ?? "Request could not be completed." });

        return Ok(new { message = "Password saved. You can sign in now." });
    }
}
