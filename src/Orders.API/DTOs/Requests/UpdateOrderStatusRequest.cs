using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs.Requests;

public class UpdateOrderStatusRequest
{
    [Required(ErrorMessage = "El estado es requerido.")]
    [RegularExpression("^(Pendiente|Confirmada|Enviada|Entregada|Cancelada)$", ErrorMessage = "El estado debe ser uno de los siguientes: Pendiente, Confirmada, Enviada, Entregada o Cancelada.")]
    public string Estado { get; set; } = string.Empty;
}
