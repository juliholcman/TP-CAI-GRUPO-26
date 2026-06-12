using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Data;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La connection string 'DefaultConnection' no está configurada.");
    }

    public void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        connection.Execute(
            """
            CREATE TABLE IF NOT EXISTS products (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                descripcion TEXT NULL,
                precio REAL NOT NULL,
                stock INTEGER NOT NULL,
                categoria TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL
            );
            """);

        SeedInitialProducts(connection);
    }

    private static void SeedInitialProducts(SqliteConnection connection)
    {
        const string sql =
            """
            INSERT OR IGNORE INTO products (
                id,
                nombre,
                descripcion,
                precio,
                stock,
                categoria,
                fecha_creacion
            )
            VALUES (
                @Id,
                @Nombre,
                @Descripcion,
                @Precio,
                @Stock,
                @Categoria,
                @FechaCreacion
            );
            """;

        var products = new[]
        {
            new Product
            {
                Id = Guid.Parse("b69b109d-9c5c-4f68-9942-a0ba2f4710b1"),
                Nombre = "Notebook Lenovo IdeaPad",
                Descripcion = "Notebook para trabajo y estudio",
                Precio = 899999.99m,
                Stock = 12,
                Categoria = "Tecnologia",
                FechaCreacion = new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new Product
            {
                Id = Guid.Parse("3b1c1b9f-5c49-4944-b6ce-d6edc40a42a7"),
                Nombre = "Mouse Logitech M280",
                Descripcion = "Mouse inalambrico ergonomico",
                Precio = 24999.50m,
                Stock = 35,
                Categoria = "Accesorios",
                FechaCreacion = new DateTime(2026, 1, 12, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        connection.Execute(sql, products.Select(ToParameters));
    }

    private static object ToParameters(Product product)
    {
        return new
        {
            Id = product.Id.ToString(),
            product.Nombre,
            product.Descripcion,
            Precio = Convert.ToDouble(product.Precio, CultureInfo.InvariantCulture),
            product.Stock,
            product.Categoria,
            FechaCreacion = product.FechaCreacion.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
        };
    }
}
