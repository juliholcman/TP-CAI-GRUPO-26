using Microsoft.AspNetCore.Diagnostics;
using Cart.API.DTOs;

namespace Cart.API.ExceptionHandlers;

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
            "Unhandled Cart API error with correlation id {CorrelationId}",
            correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Error interno del servidor",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Error interno al procesar el carrito.",
            Instance = context.Request.Path.Value,
            ErrorCode = "CRT-005",
            ErrorMessage = "Error interno al procesar el carrito.",
            CorrelationId = correlationId
        }, cancellationToken);

        return true;
    }
}
