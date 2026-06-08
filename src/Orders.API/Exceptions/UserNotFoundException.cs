using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class UserNotFoundException : OrdersApiException
{
    public UserNotFoundException() 
        : base(StatusCodes.Status404NotFound, "ORD-003", "Usuario no encontrado al crear la orden.", "Not Found")
    {
    }
}
