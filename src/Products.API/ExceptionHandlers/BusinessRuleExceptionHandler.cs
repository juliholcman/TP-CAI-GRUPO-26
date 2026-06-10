using Microsoft.AspNetCore.Diagnostics;
using Products.API.Exceptions;

namespace Products.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex)
            return false;

        var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? string.Empty;

        context.Response.StatusCode = StatusCodes.Status409Conflict;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            title = "Conflict",
            status = 409,
            detail = "Hubo un conflicto de reglas de negocio.",
            instance = context.Request.Path.Value,
            correlationId,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message
        }, cancellationToken);

        return true;
    }
}
