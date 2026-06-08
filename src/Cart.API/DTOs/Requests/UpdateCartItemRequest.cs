using System.ComponentModel.DataAnnotations;

namespace Cart.API.DTOs.Requests;

public class UpdateCartItemRequest
{
    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    public int Cantidad { get; set; }
}
