namespace Cart.API.DTOs.Responses;

public class CartResponse
{
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = [];
    public decimal Total { get; set; }
}
