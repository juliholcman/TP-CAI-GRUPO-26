using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Notifications.API.Exceptions;

namespace Notifications.API.ExceptionHandlers
{
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException notFoundException)
            {
                return false;
            }

            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                             ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                             ?? string.Empty;

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                correlationId,
                errorCode = notFoundException.ErrorCode,
                errorMessage = notFoundException.Message
            };

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}