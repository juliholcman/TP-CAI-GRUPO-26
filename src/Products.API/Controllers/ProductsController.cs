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
    /// Obtiene todos los productos activos.
    /// </summary>
    /// <response code="200">Retorna el listado de productos.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-500.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<IEnumerable<ProductResponse>> Get()
    {
        return Ok(_productService.GetAll());
    }

    /// <summary>
    /// Obtiene un producto activo por su identificador.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <response code="200">Retorna el producto solicitado.</response>
    /// <response code="400">El identificador no tiene formato GUID válido.</response>
    /// <response code="404">Producto no encontrado. Código de error: PRD-001.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-500.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ProductResponse> GetById(Guid id)
    {
        return Ok(_productService.GetById(id));
    }

    /// <summary>
    /// Crea un producto.
    /// </summary>
    /// <param name="request">Datos del producto a crear.</param>
    /// <response code="201">Producto creado correctamente.</response>
    /// <response code="400">Datos inválidos. Código de error: PRD-002.</response>
    /// <response code="409">Ya existe el producto. Código de error: PRD-003.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-500.</response>
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
    /// <param name="id">Identificador del producto.</param>
    /// <param name="request">Datos actualizados del producto.</param>
    /// <response code="200">Producto actualizado correctamente.</response>
    /// <response code="400">Identificador o datos inválidos. Código de error: PRD-002.</response>
    /// <response code="404">Producto no encontrado. Código de error: PRD-001.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-500.</response>
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
    /// Elimina un producto según el contrato vigente.
    /// </summary>
    /// <param name="id">Identificador del producto.</param>
    /// <response code="204">Producto eliminado correctamente.</response>
    /// <response code="400">El identificador no tiene formato GUID válido.</response>
    /// <response code="404">Producto no encontrado. Código de error: PRD-001.</response>
    /// <response code="409">El producto tiene órdenes activas. Código de error: PRD-004.</response>
    /// <response code="500">Error interno del servidor. Código de error: PRD-500.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Delete(Guid id)
    {
        _productService.Delete(id);

        return NoContent();
    }
}
