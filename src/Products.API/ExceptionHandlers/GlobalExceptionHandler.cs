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
        _logger.LogError(exception, "Ocurrió un error inesperado.");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Error interno del servidor",
            status = 500,
            detail = "Ocurrió un error inesperado en el servidor.",
            instance = context.Request.Path.Value,
            errorCode = "PRD-005",
            errorMessage = "Error interno del servidor."
        }, cancellationToken);

        return true;
    }
}
