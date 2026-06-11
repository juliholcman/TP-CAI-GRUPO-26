using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs.Requests;
using Orders.API.DTOs.Responses;
using Orders.API.Services;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Lista las órdenes registradas, con filtro opcional por ID de usuario.
    /// </summary>
    /// <param name="usuarioId">ID del usuario para filtrar las órdenes (opcional).</param>
    /// <returns>El listado de órdenes correspondientes al filtro.</returns>
    /// <response code="200">Retorna la lista de órdenes.</response>
    /// <response code="400">El filtro usuarioId no tiene formato GUID válido.</response>
    /// <response code="500">Error interno del servidor o de persistencia (Mapea a error ORD-007).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<OrderResponse>> Get([FromQuery] Guid? usuarioId)
    {
        var response = _orderService.GetOrders(usuarioId);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene el detalle de una orden específica por su ID.
    /// </summary>
    /// <param name="id">ID de la orden (Guid).</param>
    /// <returns>El detalle completo de la orden solicitada.</returns>
    /// <response code="200">Retorna la orden encontrada.</response>
    /// <response code="400">El ID no tiene formato GUID válido.</response>
    /// <response code="404">Si el ID de la orden no existe (Mapea a error ORD-001).</response>
    /// <response code="500">Error interno del servidor o de persistencia (Mapea a error ORD-007).</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<OrderResponse> GetById(Guid id)
    {
        var response = _orderService.GetOrderById(id);
        return Ok(response);
    }

    /// <summary>
    /// Crea una nueva orden de compra.
    /// </summary>
    /// <param name="request">Los detalles de la orden a crear (ID de usuario e ítems).</param>
    /// <returns>La orden creada con su ID generado y el total calculado.</returns>
    /// <response code="201">La orden se creó con éxito.</response>
    /// <response code="400">Si los datos de la orden son inválidos (Mapea a error ORD-002).</response>
    /// <response code="404">Si el usuario o algún producto no existen (Mapea a errores ORD-003 u ORD-004).</response>
    /// <response code="422">Si la cantidad solicitada supera el stock disponible (Mapea a error ORD-005).</response>
    /// <response code="500">Error interno del servidor o de persistencia (Mapea a error ORD-007).</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<OrderResponse> Create([FromBody] CreateOrderRequest request)
    {
        var response = _orderService.CreateOrder(request);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Actualiza el estado de una orden de compra.
    /// </summary>
    /// <param name="id">ID de la orden (Guid).</param>
    /// <param name="request">El nuevo estado solicitado.</param>
    /// <returns>La confirmación de la actualización con la fecha correspondiente.</returns>
    /// <response code="200">El estado se actualizó correctamente.</response>
    /// <response code="400">Si el estado enviado es inválido (Mapea a error ORD-002).</response>
    /// <response code="404">Si el ID de la orden no existe (Mapea a error ORD-001).</response>
    /// <response code="409">Si la transición de estado no es válida (Mapea a error ORD-006).</response>
    /// <response code="500">Error interno del servidor o de persistencia (Mapea a error ORD-007).</response>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(UpdateOrderStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<UpdateOrderStatusResponse> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var response = _orderService.UpdateOrderStatus(id, request);
        return Ok(response);
    }
}
