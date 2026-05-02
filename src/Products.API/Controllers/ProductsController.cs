using Microsoft.AspNetCore.Mvc;
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
}
