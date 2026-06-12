using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class OrdersNotFoundException : OrdersApiException
{
    public OrdersNotFoundException(string errorCode, string message)
        : base(StatusCodes.Status404NotFound, errorCode, message, "Orden no encontrada")
    {
    }
}
