namespace Orders.API.DTOs.Responses;

public class OrderItemResponse
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
