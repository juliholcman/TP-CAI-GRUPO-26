using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs.Requests;

public class CreateProductRequest
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre del producto no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "La categoría del producto es obligatoria.")]
    public string Categoria { get; set; } = string.Empty;
}
