using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs.Requests;
using Products.API.DTOs.Responses;
using Products.API.Services;

namespace Products.API.Controllers;

/// <summary>
/// Gestión del catálogo de productos.
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
    /// <response code="200">Retorna el listado de productos activos.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-005.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> Get()
    {
        return Ok(await _productService.GetAllAsync());
    }

    /// <summary>
    /// Obtiene un producto activo por su identificador único.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <response code="200">Retorna el producto solicitado.</response>
    /// <response code="400">El identificador no tiene formato GUID válido.</response>
    /// <response code="404">Producto no encontrado. Código de error: PRD-001.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-005.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id)
    {
        return Ok(await _productService.GetByIdAsync(id));
    }

    /// <summary>
    /// Crea un nuevo producto en el catálogo.
    /// </summary>
    /// <param name="request">Datos del producto a crear.</param>
    /// <response code="201">Producto creado correctamente.</response>
    /// <response code="400">Datos de la solicitud inválidos. Código de error: PRD-002.</response>
    /// <response code="409">Ya existe un producto con ese nombre en la categoría. Código de error: PRD-003.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-005.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
    {
        var response = await _productService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// </summary>
    /// <param name="id">Identificador del producto a actualizar.</param>
    /// <param name="request">Datos actualizados del producto.</param>
    /// <response code="200">Producto actualizado correctamente.</response>
    /// <response code="400">Identificador o datos de la solicitud inválidos. Código de error: PRD-002.</response>
    /// <response code="404">Producto no encontrado. Código de error: PRD-001.</response>
    /// <response code="409">Ya existe un producto con ese nombre en la categoría. Código de error: PRD-003.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-005.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var response = await _productService.UpdateAsync(id, request);

        return Ok(response);
    }

    /// <summary>
    /// Elimina lógicamente un producto (soft delete).
    /// </summary>
    /// <param name="id">Identificador del producto a eliminar.</param>
    /// <response code="204">Producto eliminado correctamente.</response>
    /// <response code="400">El identificador no tiene formato GUID válido.</response>
    /// <response code="404">Producto no encontrado. Código de error: PRD-001.</response>
    /// <response code="409">El producto tiene órdenes activas y no puede eliminarse. Código de error: PRD-004.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-005.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _productService.DeleteAsync(id);

        return NoContent();
    }
}
