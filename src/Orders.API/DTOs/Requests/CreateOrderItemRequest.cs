using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs.Requests;

public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "El ID de producto es obligatorio.")]
    public Guid ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}
