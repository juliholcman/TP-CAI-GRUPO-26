using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs.Requests;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "El ID de usuario es obligatorio.")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "La lista de ítems es obligatoria.")]
    [MinLength(1, ErrorMessage = "La orden debe contener al menos un ítem.")]
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}
