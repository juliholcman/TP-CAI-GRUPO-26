using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs.Requests;
using Notifications.API.DTOs.Responses;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Send([FromBody] NotificationRequest request)
    {
        var response = _notificationService.Send(request);
        // Retornamos 201 Created indicando que se creó exitosamente
        return CreatedAtAction(nameof(Get), new { userId = response.UsuarioId }, response);
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get([FromRoute] Guid userId)
    {
        var response = _notificationService.GetByUserId(userId);
        return Ok(response);
    }
}
