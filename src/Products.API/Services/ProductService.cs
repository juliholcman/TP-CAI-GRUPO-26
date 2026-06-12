using Products.API.Data.Repositories;
using Products.API.DTOs.Requests;
using Products.API.DTOs.Responses;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public class ProductService
{
    private readonly ProductRepository _productRepository;

    public ProductService(ProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IReadOnlyCollection<ProductResponse>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(ToResponse).ToArray();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await GetExistingProductAsync(id);
        return ToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        ValidateProductData(request.Nombre, request.Precio, request.Stock, request.Categoria);

        if (await _productRepository.ExistsByNameAndCategoryAsync(request.Nombre, request.Categoria))
        {
            throw new ConflictException(
                "PRD-003",
                "Ya existe un producto con ese nombre en la categoría.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria,
            FechaCreacion = DateTime.UtcNow
        };

        await _productRepository.CreateAsync(product);
        return ToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        ValidateProductData(request.Nombre, request.Precio, request.Stock, request.Categoria);

        var product = await GetExistingProductAsync(id);

        if (await _productRepository.ExistsByNameAndCategoryAsync(request.Nombre, request.Categoria, id))
        {
            throw new ConflictException(
                "PRD-003",
                "Ya existe un producto con ese nombre en la categoría.");
        }

        product.Nombre = request.Nombre;
        product.Descripcion = request.Descripcion;
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria;

        await _productRepository.UpdateAsync(product);
        return ToResponse(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        await GetExistingProductAsync(id);

        if (await _productRepository.HasActiveOrdersAsync(id))
        {
            throw new BusinessRuleException(
                "PRD-004",
                "El producto tiene órdenes activas y no puede eliminarse.");
        }

        await _productRepository.DeleteAsync(id);
    }

    private async Task<Product> GetExistingProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        return product
            ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");
    }

    private static void ValidateProductData(string nombre, decimal precio, int stock, string categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ValidationException("PRD-002", "El nombre del producto es obligatorio.");

        if (precio <= 0)
            throw new ValidationException("PRD-002", "El precio del producto debe ser mayor que cero.");

        if (stock < 0)
            throw new ValidationException("PRD-002", "El stock del producto no puede ser negativo.");

        if (string.IsNullOrWhiteSpace(categoria))
            throw new ValidationException("PRD-002", "La categoría del producto es obligatoria.");
    }

    private static ProductResponse ToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Nombre = product.Nombre,
            Descripcion = product.Descripcion,
            Precio = product.Precio,
            Stock = product.Stock,
            Categoria = product.Categoria,
            FechaCreacion = product.FechaCreacion
        };
    }
}
