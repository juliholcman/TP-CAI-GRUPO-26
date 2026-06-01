namespace Cart.API.DTOs.Responses;

public class CartItemResponse
{
    public Guid ProductId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}
