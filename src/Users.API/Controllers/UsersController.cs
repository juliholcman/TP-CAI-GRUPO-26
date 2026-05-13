using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs.Requests;
using Users.API.DTOs.Responses;
using Users.API.Services;

namespace Users.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Registra un usuario nuevo.
    /// </summary>
    /// <remarks>
    /// Errores posibles: USR-002 para datos inválidos, USR-001 para email duplicado y USR-006 para error interno.
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<UserResponse> Register([FromBody] RegisterUserRequest request)
    {
        var response = _userService.Register(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Autentica un usuario registrado.
    /// </summary>
    /// <remarks>
    /// Errores posibles: USR-002 para datos inválidos, USR-003 para credenciales incorrectas, USR-004 para bloqueo por intentos, USR-005 para bloqueo por fraude y USR-006 para error interno.
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<LoginResponse> Login([FromBody] LoginUserRequest request)
    {
        return Ok(_userService.Login(request));
    }
}
