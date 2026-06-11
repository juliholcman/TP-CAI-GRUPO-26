using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs.Requests;
using Notifications.API.DTOs.Responses;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

/// <summary>
/// Gestión de envío y consulta de notificaciones.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Envía una notificación a un usuario.
    /// </summary>
    /// <remarks>
    /// <b>Tipos permitidos:</b> Email, Push, SMS.<br/>
    /// <b>Errores posibles:</b><br/>
    /// - <c>NTF-001</c>: El usuario no existe.<br/>
    /// - <c>NTF-002</c>: Datos de la solicitud inválidos (Tipo incorrecto, campos vacíos, UsuarioId vacío).<br/>
    /// - <c>NTF-004</c>: Error interno del servidor.
    /// </remarks>
    /// <param name="request">Datos de la notificación a enviar.</param>
    /// <response code="201">Notificación creada y enviada correctamente.</response>
    /// <response code="400">
    /// Datos inválidos.<br/>
    /// <b>NTF-002</b>: Tipo no permitido, Mensaje vacío, UsuarioId vacío.
    /// </response>
    /// <response code="404">
    /// Usuario no encontrado.<br/>
    /// <b>NTF-001</b>: El UsuarioId proporcionado no corresponde a ningún usuario registrado.
    /// </response>
    /// <response code="500">
    /// Error interno.<br/>
    /// <b>NTF-004</b>: Error inesperado del servidor.
    /// </response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Send([FromBody] NotificationRequest request)
    {
        var response = _notificationService.Send(request);
        return CreatedAtAction(nameof(Get), new { userId = response.UsuarioId }, response);
    }

    /// <summary>
    /// Obtiene todas las notificaciones de un usuario.
    /// </summary>
    /// <remarks>
    /// <b>Errores posibles:</b><br/>
    /// - <c>NTF-003</c>: El usuario no tiene notificaciones registradas.<br/>
    /// - <c>NTF-004</c>: Error interno del servidor.
    /// </remarks>
    /// <param name="userId">Identificador único del usuario.</param>
    /// <response code="200">Lista de notificaciones del usuario.</response>
    /// <response code="404">
    /// No se encontraron notificaciones.<br/>
    /// <b>NTF-003</b>: El usuario no tiene notificaciones registradas.
    /// </response>
    /// <response code="500">
    /// Error interno.<br/>
    /// <b>NTF-004</b>: Error inesperado del servidor.
    /// </response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Get([FromRoute] Guid userId)
    {
        var response = _notificationService.GetByUserId(userId);
        return Ok(response);
    }
}
