using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class UserNotFoundException : OrdersApiException
{
    public UserNotFoundException() 
        : base(StatusCodes.Status404NotFound, "ORD-003", "No se encontró el usuario indicado para crear la orden.", "Usuario no encontrado")
    {
    }
}
