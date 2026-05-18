using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs.Requests;

public class CreateProductRequest
{
    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Precio { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    public string Categoria { get; set; } = string.Empty;
}
