using Microsoft.AspNetCore.Http;

namespace Orders.API.Exceptions;

public class ProductNotFoundException : OrdersApiException
{
    public ProductNotFoundException(Guid productoId) 
        : base(StatusCodes.Status404NotFound, "ORD-004", $"No se encontró el producto con identificador '{productoId}' al crear la orden.", "Producto no encontrado")
    {
    }
}
