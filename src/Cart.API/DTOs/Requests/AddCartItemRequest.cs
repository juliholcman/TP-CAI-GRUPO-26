using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs.Requests;

public class AddCartItemRequest
{
    [Required(ErrorMessage = "El identificador del producto es obligatorio.")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero.")]
    public int Cantidad { get; set; }
}
