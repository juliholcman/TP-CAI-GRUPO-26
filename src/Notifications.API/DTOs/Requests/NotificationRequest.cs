namespace Notifications.API.DTOs.Requests;

public class NotificationRequest
{
    public Guid UsuarioId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
