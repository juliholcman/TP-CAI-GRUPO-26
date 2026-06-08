namespace Orders.API.DTOs.Responses;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public IEnumerable<OrderItemResponse> Items { get; set; } = Array.Empty<OrderItemResponse>();
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
