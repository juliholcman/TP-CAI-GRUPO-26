using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs.Requests;
using Products.API.DTOs.Responses;
using Products.API.Services;

namespace Products.API.Controllers;

/// <summary>
/// Gestión de productos del catálogo.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Obtiene todos los productos activos (no eliminados).
    /// </summary>
    /// <returns>Lista de productos activos.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<ProductResponse>> Get()
    {
        return Ok(_productService.GetAll());
    }

    /// <summary>
    /// Obtiene un producto por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <returns>El producto encontrado.</returns>
    /// <response code="200">Producto encontrado correctamente.</response>
    /// <response code="404">PRD-001: Producto no encontrado.</response>
    /// <response code="500">PRD-005: Error interno del servidor.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ProductResponse> GetById(Guid id)
    {
        return Ok(_productService.GetById(id));
    }

    /// <summary>
    /// Crea un nuevo producto en el catálogo.
    /// </summary>
    /// <param name="request">Datos del producto a crear.</param>
    /// <returns>El producto creado.</returns>
    /// <response code="201">Producto creado correctamente.</response>
    /// <response code="400">PRD-002: Datos de la solicitud inválidos.</response>
    /// <response code="409">PRD-003: Ya existe un producto con ese nombre en la categoría.</response>
    /// <response code="500">PRD-005: Error interno del servidor.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ProductResponse> Create([FromBody] CreateProductRequest request)
    {
        var response = _productService.Create(request);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto a actualizar.</param>
    /// <param name="request">Datos actualizados del producto.</param>
    /// <returns>El producto actualizado.</returns>
    /// <response code="200">Producto actualizado correctamente.</response>
    /// <response code="400">PRD-002: Datos de la solicitud inválidos.</response>
    /// <response code="404">PRD-001: Producto no encontrado.</response>
    /// <response code="500">PRD-005: Error interno del servidor.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ProductResponse> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var response = _productService.Update(id, request);

        return Ok(response);
    }

    /// <summary>
    /// Elimina lógicamente un producto (soft delete).
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <returns>Sin contenido.</returns>
    /// <response code="204">Producto eliminado correctamente.</response>
    /// <response code="404">PRD-001: Producto no encontrado.</response>
    /// <response code="409">PRD-004: El producto tiene órdenes activas y no puede eliminarse.</response>
    /// <response code="500">PRD-005: Error interno del servidor.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Delete(Guid id)
    {
        _productService.Delete(id);

        return NoContent();
    }
}
