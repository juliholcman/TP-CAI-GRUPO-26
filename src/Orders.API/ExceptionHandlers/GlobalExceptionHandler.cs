using Microsoft.AspNetCore.Diagnostics;
using Orders.API.DTOs.Responses;

namespace Orders.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
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
            "Unhandled Orders API error with correlation id {CorrelationId}",
            correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Error interno del servidor",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Error interno al procesar la orden.",
            Instance = context.Request.Path.Value,
            ErrorCode = "ORD-007",
            ErrorMessage = "Error interno al procesar la orden.",
            CorrelationId = correlationId
        }, cancellationToken);

        return true;
    }
}
