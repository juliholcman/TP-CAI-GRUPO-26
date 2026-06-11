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

    /// <summary>
    /// Devuelve todos los productos activos (DeletedAt == null).
    /// </summary>
    public async Task<IReadOnlyCollection<ProductResponse>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(ToResponse).ToArray();
    }

    /// <summary>
    /// Busca un producto activo por su Id.
    /// Lanza NotFoundException (PRD-001) si no existe o fue eliminado.
    /// </summary>
    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await GetExistingProductAsync(id);
        return ToResponse(product);
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// Lanza ValidationException (PRD-002) si los datos son inválidos.
    /// Lanza ConflictException (PRD-003) si ya existe uno con el mismo Nombre y Categoria.
    /// </summary>
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        ValidateProductData(request.Nombre, request.Precio, request.Stock, request.Categoria);

        if (await _productRepository.ExistsByNameAndCategoryAsync(request.Nombre, request.Categoria))
        {
            throw new ConflictException(
                "PRD-003",
                $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
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

    /// <summary>
    /// Actualiza un producto existente.
    /// Lanza NotFoundException (PRD-001) si no existe o fue eliminado.
    /// Lanza ValidationException (PRD-002) si los datos son inválidos.
    /// Lanza ConflictException (PRD-003) si ya existe otro producto con el mismo Nombre y Categoria.
    /// </summary>
    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        ValidateProductData(request.Nombre, request.Precio, request.Stock, request.Categoria);

        var product = await GetExistingProductAsync(id);

        if (await _productRepository.ExistsByNameAndCategoryAsync(request.Nombre, request.Categoria, id))
        {
            throw new ConflictException(
                "PRD-003",
                $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");
        }

        product.Nombre = request.Nombre;
        product.Descripcion = request.Descripcion;
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria;

        await _productRepository.UpdateAsync(product);
        return ToResponse(product);
    }

    /// <summary>
    /// Elimina lógicamente un producto (soft delete marcando DeletedAt).
    /// Lanza NotFoundException (PRD-001) si no existe o ya fue eliminado.
    /// Lanza BusinessRuleException (PRD-004) si el producto tiene órdenes activas.
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        await GetExistingProductAsync(id);

        if (await _productRepository.HasActiveOrdersAsync(id))
        {
            throw new BusinessRuleException(
                "PRD-004",
                "El producto tiene órdenes activas y no puede eliminarse.");
        }

        await _productRepository.SoftDeleteAsync(id);
    }

    private async Task<Product> GetExistingProductAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        return product
            ?? throw new NotFoundException("PRD-001", "Producto no encontrado.");
    }

    private static void ValidateProductData(string nombre, decimal precio, int stock, string categoria)
    {
        if (string.IsNullOrWhiteSpace(nombre)
            || precio <= 0
            || stock < 0
            || string.IsNullOrWhiteSpace(categoria))
        {
            throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");
        }
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
