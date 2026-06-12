using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class OrdersConflictException : OrdersApiException
{
    public OrdersConflictException(string message)
        : base(StatusCodes.Status409Conflict, "ORD-006", message, "Cambio de estado no permitido")
    {
    }
}
