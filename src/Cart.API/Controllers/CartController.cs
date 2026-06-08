using Microsoft.AspNetCore.Mvc;
using Cart.API.DTOs;
using Cart.API.DTOs.Requests;
using Cart.API.DTOs.Responses;
using Cart.API.Services;

namespace Cart.API.Controllers;

/// <summary>
/// Controlador para gestionar el carrito de compras de los usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="CartController"/>.
    /// </summary>
    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Obtiene el carrito activo del usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>El carrito del usuario.</returns>
    /// <response code="200">Retorna el carrito del usuario.</response>
    /// <response code="404">Si el usuario no tiene un carrito activo. Código de error: CRT-001</response>
    /// <response code="500">Error inesperado en servicio o persistencia. Código de error: CRT-005</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<CartResponse> GetCart(Guid userId)
    {
        var response = _cartService.GetCart(userId);
        return Ok(response);
    }

    /// <summary>
    /// Agrega un producto al carrito del usuario. Si el usuario no tiene un carrito activo, crea uno.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="request">Datos del producto a agregar.</param>
    /// <returns>El carrito actualizado.</returns>
    /// <response code="200">Retorna el carrito actualizado.</response>
    /// <response code="400">Si la cantidad es menor o igual a cero. Código de error: CRT-004</response>
    /// <response code="404">Si el producto no existe en el catálogo. Código de error: CRT-002</response>
    /// <response code="422">Si la cantidad supera el stock disponible. Código de error: CRT-003</response>
    /// <response code="500">Error inesperado en servicio o persistencia. Código de error: CRT-005</response>
    [HttpPost("{userId}/items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartResponse>> AddItem(Guid userId, [FromBody] AddCartItemRequest request)
    {
        var response = await _cartService.AddItemAsync(userId, request);
        return Ok(response);
    }

    /// <summary>
    /// Actualiza la cantidad de un producto existente en el carrito del usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="productId">Identificador del producto a actualizar.</param>
    /// <param name="request">Nueva cantidad del producto.</param>
    /// <returns>El carrito actualizado.</returns>
    /// <response code="200">Retorna el carrito actualizado.</response>
    /// <response code="400">Si la cantidad es menor o igual a cero. Código de error: CRT-004</response>
    /// <response code="404">Si el carrito no está activo (CRT-001) o el producto no está en el carrito/catálogo (CRT-002).</response>
    /// <response code="422">Si la cantidad supera el stock disponible. Código de error: CRT-003</response>
    /// <response code="500">Error inesperado en servicio o persistencia. Código de error: CRT-005</response>
    [HttpPut("{userId}/items/{productId}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartResponse>> UpdateItem(Guid userId, Guid productId, [FromBody] UpdateCartItemRequest request)
    {
        var response = await _cartService.UpdateItemAsync(userId, productId, request);
        return Ok(response);
    }

    /// <summary>
    /// Quita un producto del carrito del usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="productId">Identificador del producto a remover.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Producto removido con éxito.</response>
    /// <response code="404">Si el carrito no está activo (CRT-001) o el producto no está en el carrito (CRT-002).</response>
    /// <response code="500">Error inesperado en servicio o persistencia. Código de error: CRT-005</response>
    [HttpDelete("{userId}/items/{productId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult RemoveItem(Guid userId, Guid productId)
    {
        _cartService.RemoveItem(userId, productId);
        return NoContent();
    }

    /// <summary>
    /// Vacía el carrito completo del usuario de forma activa.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <returns>No content.</returns>
    /// <response code="204">Carrito vaciado y eliminado con éxito.</response>
    /// <response code="404">Si el usuario no tiene un carrito activo. Código de error: CRT-001</response>
    /// <response code="500">Error inesperado en servicio o persistencia. Código de error: CRT-005</response>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult ClearCart(Guid userId)
    {
        _cartService.ClearCart(userId);
        return NoContent();
    }
}
