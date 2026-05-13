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

    public IEnumerable<ProductResponse> GetAll()
    {
        return _products
            .Where(product => product.DeletedAt is null)
            .Select(product => new ProductResponse
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            });
    }

    public ProductResponse GetById(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.DeletedAt is null);
        if (product is null)
        {
            throw new NotFoundException("PRD-001", "Producto no encontrado.");
        }

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
