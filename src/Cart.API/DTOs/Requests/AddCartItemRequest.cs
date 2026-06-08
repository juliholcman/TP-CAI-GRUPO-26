using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs.Requests;

public class AddCartItemRequest
{
    [Required(ErrorMessage = "El id de producto es obligatorio.")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    public int Cantidad { get; set; }
}
