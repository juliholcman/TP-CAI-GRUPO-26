using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException businessException)
            {
                return false;
            }

            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                             ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                             ?? string.Empty;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Solicitud inválida",
                status = 400,
                detail = "La solicitud contiene datos inválidos o no permitidos.",
                instance = context.Request.Path.Value,
                correlationId,
                errorCode = businessException.ErrorCode,
                errorMessage = businessException.Message
            };

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
