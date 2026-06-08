using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs.Requests;

public class LoginUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
