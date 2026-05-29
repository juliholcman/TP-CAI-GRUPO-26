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
            // 1. Verificamos si la alarma es de tipo "Recurso no encontrado"
            if (exception is not NotFoundException notFoundException)
            {
                return false; // Si no es de este tipo, dejamos que otro handler se encargue
            }

            // 2. Configuramos la respuesta como 404
            context.Response.StatusCode = StatusCodes.Status404NotFound;

            // 3. Armamos el objeto JSON tal cual pide el PDF
            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4", // Enlace estándar de error 404
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                errorCode = notFoundException.ErrorCode, // Aquí va el NTF-001 o NTF-003
                errorMessage = notFoundException.Message
            };

            // 4. Enviamos la respuesta al usuario
            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Confirmamos que ya manejamos el error
        }
    }
}