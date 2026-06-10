using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs.Requests;

public class CreateProductRequest
{
    [Required(ErrorMessage = "El campo Nombre es requerido.")]
    [MaxLength(100, ErrorMessage = "El campo Nombre no puede superar los 100 caracteres.")]
    [MinLength(1, ErrorMessage = "El campo Nombre no puede ser vacío.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El campo Nombre no puede ser solo espacios en blanco.")]
    public string Nombre { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "El campo Descripcion no puede superar los 500 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El campo Precio es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El campo Precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El campo Stock es requerido.")]
    [Range(0, int.MaxValue, ErrorMessage = "El campo Stock debe ser mayor o igual a 0.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "El campo Categoria es requerido.")]
    [MinLength(1, ErrorMessage = "El campo Categoria no puede ser vacío.")]
    [RegularExpression(@".*\S.*", ErrorMessage = "El campo Categoria no puede ser solo espacios en blanco.")]
    public string Categoria { get; set; } = string.Empty;
}
