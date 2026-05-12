using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Part.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Request failed: {Path}", httpContext.Request.Path);

        var statusCode = MapStatusCode(exception);

        var detail = ResolveDetail(exception, statusCode);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = TitleFor(statusCode),
            Detail = detail,
            Instance = httpContext.Request.Path.Value
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }

    private static int MapStatusCode(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentNullException => StatusCodes.Status400BadRequest,
            ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            DbUpdateConcurrencyException => StatusCodes.Status409Conflict,
            DbUpdateException => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private string? ResolveDetail(Exception exception, int statusCode)
    {
        if (environment.IsDevelopment())
            return exception.Message;

        return statusCode switch
        {
            StatusCodes.Status400BadRequest or StatusCodes.Status404NotFound or StatusCodes.Status403Forbidden =>
                exception.Message,
            StatusCodes.Status409Conflict =>
                "The record was modified by another operation (for example a concurrent stock change). Retry the request.",
            StatusCodes.Status503ServiceUnavailable =>
                "The service is temporarily unavailable. Please try again later.",
            _ => "An unexpected error occurred."
        };
    }

    private static string TitleFor(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status503ServiceUnavailable => "Service Unavailable",
            _ => "Server Error"
        };
    }
}
