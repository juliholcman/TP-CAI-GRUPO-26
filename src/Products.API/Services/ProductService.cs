using Products.API.DTOs.Requests;
using Products.API.DTOs.Responses;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public class ProductService
{
    private readonly List<Product> _products =
    [
        new()
        {
            Id = Guid.Parse("b69b109d-9c5c-4f68-9942-a0ba2f4710b1"),
            Nombre = "Notebook Lenovo IdeaPad",
            Descripcion = "Notebook para trabajo y estudio",
            Precio = 899999.99m,
            Stock = 12,
            Categoria = "Tecnologia",
            FechaCreacion = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            DeletedAt = null
        },
        new()
        {
            Id = Guid.Parse("3b1c1b9f-5c49-4944-b6ce-d6edc40a42a7"),
            Nombre = "Mouse Logitech M280",
            Descripcion = "Mouse inalambrico ergonomico",
            Precio = 24999.50m,
            Stock = 35,
            Categoria = "Accesorios",
            FechaCreacion = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc),
            DeletedAt = null
        },
        new()
        {
            Id = Guid.Parse("975c2b86-c5d7-4921-93fe-96a42f8323f6"),
            Nombre = "Teclado mecanico",
            Descripcion = "Teclado mecanico compacto",
            Precio = 79999.00m,
            Stock = 0,
            Categoria = "Accesorios",
            FechaCreacion = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            DeletedAt = DateTime.UtcNow
        }
    ];

    /// <summary>
    /// Devuelve solo productos activos (DeletedAt == null).
    /// </summary>
    public IEnumerable<ProductResponse> GetAll()
    {
        return _products
            .Where(product => product.DeletedAt is null)
            .Select(MapToResponse);
    }

    /// <summary>
    /// Busca un producto activo por su Id.
    /// Lanza NotFoundException (PRD-001) si no existe o fue eliminado.
    /// </summary>
    public ProductResponse GetById(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.DeletedAt is null);
        if (product is null)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        return MapToResponse(product);
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// Lanza ConflictException (PRD-003) si ya existe uno con el mismo Nombre y Categoria.
    /// </summary>
    public ProductResponse Create(CreateProductRequest request)
    {
        var exists = _products.Any(p =>
            p.DeletedAt == null &&
            p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) &&
            p.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase));

        if (exists)
            throw new ConflictException("PRD-003", "Ya existe un producto con ese nombre en la categoría.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria,
            FechaCreacion = DateTime.UtcNow,
            DeletedAt = null
        };

        _products.Add(product);

        return MapToResponse(product);
    }

    /// <summary>
    /// Actualiza un producto existente.
    /// Lanza NotFoundException (PRD-001) si no existe o fue eliminado.
    /// La validación de campos se realiza en capa de DataAnnotations (PRD-002).
    /// </summary>
    public ProductResponse Update(Guid id, UpdateProductRequest request)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.DeletedAt is null);
        if (product is null)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        product.Nombre = request.Nombre;
        product.Descripcion = request.Descripcion;
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria;

        return MapToResponse(product);
    }

    /// <summary>
    /// Elimina lógicamente un producto (soft delete marcando DeletedAt).
    /// Lanza NotFoundException (PRD-001) si no existe o ya fue eliminado.
    ///
    /// STUB PRD-004: La verificación de órdenes activas requiere integración con Orders.API,
    /// que aún no está disponible. Se simula con un ID fijo para demostración del contrato.
    /// TODO: reemplazar por llamada HTTP a Orders.API cuando esté disponible.
    /// </summary>
    public void Delete(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.DeletedAt is null);
        if (product is null)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

        // STUB TEMPORAL – PRD-004:
        // Sin integración real con Orders.API, se simula que el producto con este ID
        // tiene órdenes activas. Reemplazar por validación real cuando Orders.API esté disponible.
        if (id == Guid.Parse("3b1c1b9f-5c49-4944-b6ce-d6edc40a42a7"))
        {
            throw new BusinessRuleException("PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");
        }

        product.DeletedAt = DateTime.UtcNow;
    }

    private static ProductResponse MapToResponse(Product product) => new()
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
