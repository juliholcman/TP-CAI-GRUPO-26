using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Notifications.API.ExceptionHandlers;

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
        var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? string.Empty;

        _logger.LogError(
            exception,
            "Error inesperado. CorrelationId: {CorrelationId}",
            correlationId);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrió un error inesperado al procesar la solicitud.",
            instance = context.Request.Path.Value,
            correlationId,
            errorCode = "NTF-004",
            errorMessage = "Error interno del servidor."
        };

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
