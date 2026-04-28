using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Vechile_Part.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Log the error so we can see it in the terminal
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        // 2. Create a "Problem Details" response (Standard for APIs)
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = exception.Message 
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        // 3. Send the error back to the frontend as JSON
        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Tells .NET we handled the error
    }
}