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
        _logger.LogError(exception, "Ha ocurrido un error inesperado.");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Ocurrió un error inesperado al procesar la solicitud.",
            instance = context.Request.Path.Value,
            errorCode = "NTF-500",
            errorMessage = "Error interno del servidor."
        };

        await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
