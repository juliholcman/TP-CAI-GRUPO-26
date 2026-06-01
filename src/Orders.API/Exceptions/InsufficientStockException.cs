using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class InsufficientStockException : OrdersApiException
{
    public InsufficientStockException(string message) 
        : base(StatusCodes.Status422UnprocessableEntity, "ORD-005", message, "Unprocessable Entity")
    {
    }
}
