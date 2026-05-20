using Notifications.API.DTOs.Requests;
using Notifications.API.DTOs.Responses;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

public class NotificationService
{
    private readonly List<Notification> _notifications = [];
    private readonly object _syncRoot = new();

    public NotificationResponse Send(NotificationRequest request)
    {
        ValidateNotificationRequest(request);

        lock (_syncRoot)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Mensaje = request.Mensaje.Trim(),
                Tipo = request.Tipo.Trim(),
                Estado = "Pendiente", // Estado inicial antes de procesar
                FechaEnvio = DateTime.UtcNow
            };

            try
            {
                // TODO: Aquí iría la integración con el provider real (Email, SMS, Push, etc.)
                // Como es una simulación, lo marcamos como "Enviada"
                notification.Estado = "Enviada";
            }
            catch
            {
                // Si la integración falla, manejamos el estado correspondiente
                notification.Estado = "Fallida";
                
                // Dependiendo del requerimiento, podrías registrar el error o relanzarlo
            }

            _notifications.Add(notification);

            return ToResponse(notification);
        }
    }

    public IEnumerable<NotificationResponse> GetByUserId(Guid userId)
    {
        lock (_syncRoot)
        {
            var userNotifications = _notifications
                .Where(n => n.UsuarioId == userId)
                .ToList();

            if (userNotifications.Count == 0)
            {
                throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario solicitado.");
            }

            return userNotifications.Select(ToResponse);
        }
    }

    private static void ValidateNotificationRequest(NotificationRequest request)
    {
        if (request is null || 
            request.UsuarioId == Guid.Empty || 
            string.IsNullOrWhiteSpace(request.Mensaje) || 
            string.IsNullOrWhiteSpace(request.Tipo))
        {
            // Cumpliendo con el catálogo de errores y validación
            throw new BusinessRuleException("NTF-002", "Los datos de la notificación son inválidos o faltantes.");
        }
    }

    private static NotificationResponse ToResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }
}
