using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs.Requests;

public class RegisterUserRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
