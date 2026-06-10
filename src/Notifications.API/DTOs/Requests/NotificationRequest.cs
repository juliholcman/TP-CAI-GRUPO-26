using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs.Requests;

/// <summary>
/// Datos necesarios para enviar una notificación a un usuario.
/// </summary>
public class NotificationRequest
{
    /// <summary>
    /// Identificador del usuario destinatario. No puede ser Guid.Empty.
    /// </summary>
    [Required(ErrorMessage = "UsuarioId es requerido.")]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Contenido del mensaje de la notificación.
    /// </summary>
    [Required(ErrorMessage = "Mensaje es requerido.")]
    [MaxLength(500, ErrorMessage = "Mensaje no puede superar los 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Canal de envío. Valores permitidos: Email, Push, SMS.
    /// </summary>
    [Required(ErrorMessage = "Tipo es requerido.")]
    public string Tipo { get; set; } = string.Empty;
}
