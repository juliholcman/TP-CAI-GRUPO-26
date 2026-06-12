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
    [Required(ErrorMessage = "El identificador del usuario es obligatorio.")]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Contenido del mensaje de la notificación.
    /// </summary>
    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El mensaje no puede superar los 500 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Canal de envío. Valores permitidos: Email, Push, SMS.
    /// </summary>
    [Required(ErrorMessage = "El tipo de notificación es obligatorio.")]
    public string Tipo { get; set; } = string.Empty;
}
