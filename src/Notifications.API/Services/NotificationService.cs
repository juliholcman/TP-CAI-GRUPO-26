using Notifications.API.Data.Repositories;
using Notifications.API.DTOs.Requests;
using Notifications.API.DTOs.Responses;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

public class NotificationService
{
    // Valores de Tipo permitidos según el catálogo del TP
    private static readonly HashSet<string> TiposPermitidos =
        new(StringComparer.OrdinalIgnoreCase) { "Email", "Push", "SMS" };

    private readonly NotificationRepository _repository;
    private readonly IUserValidator _userValidator;

    public NotificationService(NotificationRepository repository, IUserValidator userValidator)
    {
        _repository = repository;
        _userValidator = userValidator;
    }

    /// <summary>
    /// Envía una notificación al usuario indicado y la persiste en SQLite.
    /// Lanza <see cref="NotFoundException"/> (NTF-001) si el usuario no existe.
    /// Lanza <see cref="BusinessRuleException"/> (NTF-002) si el Tipo no es válido
    /// o si UsuarioId es Guid.Empty.
    /// </summary>
    public async Task<NotificationResponse> SendAsync(NotificationRequest request)
    {
        ValidateRequest(request);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje.Trim(),
            // Normalizamos a la capitalización canónica registrada en TiposPermitidos
            Tipo = NormalizeTipo(request.Tipo),
            Estado = "Pendiente",
            FechaEnvio = DateTime.UtcNow
        };

        try
        {
            // TODO: aquí iría la integración real con el proveedor (Email, SMS, Push).
            // Como es una simulación, marcamos la notificación como "Enviada".
            notification.Estado = "Enviada";
        }
        catch
        {
            notification.Estado = "Fallida";
        }

        await _repository.CreateAsync(notification);

        return ToResponse(notification);
    }

    /// <summary>
    /// Obtiene las notificaciones de un usuario desde SQLite.
    /// Lanza <see cref="NotFoundException"/> (NTF-003) si el usuario no tiene notificaciones.
    /// </summary>
    public async Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId)
    {
        var userNotifications = await _repository.GetByUserIdAsync(userId);

        if (userNotifications.Count == 0)
        {
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario solicitado.");
        }

        return userNotifications.Select(ToResponse);
    }

    // ─── Métodos privados ────────────────────────────────────────────────────

    private void ValidateRequest(NotificationRequest request)
    {
        // UsuarioId vacío: error de datos inválidos (NTF-002)
        if (request.UsuarioId == Guid.Empty)
        {
            throw new BusinessRuleException(
                "NTF-002",
                "UsuarioId no puede ser Guid.Empty.");
        }

        // Tipo inválido: NTF-002
        if (string.IsNullOrWhiteSpace(request.Tipo) || !TiposPermitidos.Contains(request.Tipo))
        {
            throw new BusinessRuleException(
                "NTF-002",
                $"El Tipo '{request.Tipo}' no es válido. Valores permitidos: {string.Join(", ", TiposPermitidos)}.");
        }

        // Usuario inexistente: NTF-001
        if (!_userValidator.UserExists(request.UsuarioId))
        {
            throw new NotFoundException(
                "NTF-001",
                $"El usuario con id '{request.UsuarioId}' no existe.");
        }
    }

    /// <summary>
    /// Retorna el Tipo con la capitalización canónica (e.g. "email" → "Email").
    /// </summary>
    private static string NormalizeTipo(string tipo)
    {
        // TiposPermitidos usa OrdinalIgnoreCase, así que encontraremos siempre la forma canónica
        return TiposPermitidos.TryGetValue(tipo, out var canonical) ? canonical : tipo;
    }

    private static NotificationResponse ToResponse(Notification notification) =>
        new()
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
}
