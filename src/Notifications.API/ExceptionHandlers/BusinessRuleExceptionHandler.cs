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
            // 1. Verificamos si la alarma es de tipo "Regla de Negocio"
            if (exception is not BusinessRuleException businessException)
            {
                return false; // Si no es este tipo de error, se lo pasa al siguiente handler
            }

            // 2. Configuramos la respuesta como 400 (Bad Request) 
            // según el catálogo de errores NTF-002 del PDF [cite: 235]
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            // 3. Armamos el JSON con el formato "Problem Details" que pide el TP [cite: 18, 19]
            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "La solicitud contiene datos inválidos o no permitidos.",
                instance = context.Request.Path.Value,
                errorCode = businessException.ErrorCode, // Aquí irá "NTF-002"
                errorMessage = businessException.Message
            };

            // 4. Enviamos la respuesta
            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}