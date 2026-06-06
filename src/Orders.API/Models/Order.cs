namespace Orders.API.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public OrderItem[] Items { get; set; } = Array.Empty<OrderItem>();
    public decimal Total => Items?.Sum(item => item.Cantidad * item.PrecioUnitario) ?? 0m;
    public string Estado { get; set; } = "Pendiente";
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}
