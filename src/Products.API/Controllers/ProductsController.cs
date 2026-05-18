using Microsoft.AspNetCore.Mvc;
using Products.API.DTOs.Requests;
using Products.API.DTOs.Responses;
using Products.API.Services;

namespace Products.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProductResponse>> Get()
    {
        return Ok(_productService.GetAll());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<ProductResponse> GetById(Guid id)
    {
        return Ok(_productService.GetById(id));
    }

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
}
