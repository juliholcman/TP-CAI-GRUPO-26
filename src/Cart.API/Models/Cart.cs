namespace Cart.API.Models;

public class Cart
{
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = [];

    public decimal Total => Items.Sum(item => item.Subtotal);
}
