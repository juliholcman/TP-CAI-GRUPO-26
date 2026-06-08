using Microsoft.AspNetCore.Diagnostics;
using Users.API.DTOs.Responses;

namespace Users.API.ExceptionHandlers;

public class UnhandledExceptionHandler : IExceptionHandler
{
    private readonly ILogger<UnhandledExceptionHandler> _logger;

    public UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = context.Response.Headers.TryGetValue("X-Correlation-Id", out var header)
            ? header.ToString()
            : context.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled User API error with correlation id {CorrelationId}",
            correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Error interno al procesar el usuario.",
            Instance = context.Request.Path.Value,
            ErrorCode = "USR-006",
            ErrorMessage = "Error interno al procesar el usuario.",
            CorrelationId = correlationId
        }, cancellationToken);

        return true;
    }
}
