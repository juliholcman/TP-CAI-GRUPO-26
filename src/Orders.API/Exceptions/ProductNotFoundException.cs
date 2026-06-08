using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class ProductNotFoundException : OrdersApiException
{
    public ProductNotFoundException(Guid productoId) 
        : base(StatusCodes.Status404NotFound, "ORD-004", $"Producto no encontrado al crear la orden.", "Not Found")
    {
    }
}
