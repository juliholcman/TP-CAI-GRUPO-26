namespace Notifications.API.DTOs.Responses;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; }
}
