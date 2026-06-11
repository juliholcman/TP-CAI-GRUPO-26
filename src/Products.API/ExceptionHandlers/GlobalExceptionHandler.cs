using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

namespace Products.API.ExceptionHandlers;

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
        var correlationId = context.Response.Headers.TryGetValue("X-Correlation-Id", out var cid)
            ? cid.ToString()
            : context.Request.Headers.TryGetValue("X-Correlation-Id", out var rcid)
                ? rcid.ToString()
                : string.Empty;

        _logger.LogError(exception, "Ocurrió un error inesperado. CorrelationId: {CorrelationId}", correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrió un error inesperado en el servidor.",
            instance = context.Request.Path.Value,
            correlationId,
            errorCode = "PRD-005",
            errorMessage = "Error interno al procesar el producto."
        }, cancellationToken);

        return true;
    }
}
