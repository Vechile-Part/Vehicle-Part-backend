namespace VehiclePart.Application.DTOs;

public record LoginDto(string Email, string Password);

public record StaffLoginResponse(string Token, string Role);

public record CustomerLoginResponse(string Token, string Role, Guid CustomerId);

public sealed class AuthLoginResult<T>
{
    public T? Value { get; private init; }
    public string? Error { get; private init; }
    public int StatusCode { get; private init; }

    public bool IsSuccess => Value is not null;

    public static AuthLoginResult<T> Success(T value, int statusCode = StatusCodes.Status200OK) =>
        new() { Value = value, StatusCode = statusCode };

    public static AuthLoginResult<T> Failure(string error, int statusCode) =>
        new() { Error = error, StatusCode = statusCode };
}

// Status codes without ASP.NET reference in Application — use ints
public static class StatusCodes
{
    public const int Status200OK = 200;
    public const int Status400BadRequest = 400;
    public const int Status401Unauthorized = 401;
}
