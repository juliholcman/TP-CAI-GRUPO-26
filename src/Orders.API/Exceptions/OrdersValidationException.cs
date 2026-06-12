using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class OrdersValidationException : OrdersApiException
{
    public OrdersValidationException(string message) 
        : base(StatusCodes.Status400BadRequest, "ORD-002", message, "Datos de la orden inválidos")
    {
    }
}
