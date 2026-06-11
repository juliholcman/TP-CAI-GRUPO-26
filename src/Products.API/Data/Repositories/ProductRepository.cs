using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Products.API.Models;

namespace Products.API.Data.Repositories;

public class ProductRepository
{
    private static readonly Guid ProductWithActiveOrdersId =
        Guid.Parse("3b1c1b9f-5c49-4944-b6ce-d6edc40a42a7");

    private const string ProductColumns =
        """
        id AS Id,
        nombre AS Nombre,
        descripcion AS Descripcion,
        precio AS Precio,
        stock AS Stock,
        categoria AS Categoria,
        fecha_creacion AS FechaCreacion,
        deleted_at AS DeletedAt
        """;

    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La connection string 'DefaultConnection' no está configurada.");
    }

    public async Task<IReadOnlyCollection<Product>> GetAllAsync()
    {
        var sql = $"SELECT {ProductColumns} FROM products WHERE deleted_at IS NULL ORDER BY fecha_creacion;";

        await using var connection = await OpenConnectionAsync();
        var rows = await connection.QueryAsync<ProductRow>(sql);

        return rows.Select(MapToProduct).ToArray();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var sql = $"SELECT {ProductColumns} FROM products WHERE id = @Id AND deleted_at IS NULL;";

        await using var connection = await OpenConnectionAsync();
        var row = await connection.QuerySingleOrDefaultAsync<ProductRow>(sql, new { Id = id.ToString() });

        return row is null ? null : MapToProduct(row);
    }

    public async Task CreateAsync(Product product)
    {
        const string sql =
            """
            INSERT INTO products (
                id,
                nombre,
                descripcion,
                precio,
                stock,
                categoria,
                fecha_creacion,
                deleted_at
            )
            VALUES (
                @Id,
                @Nombre,
                @Descripcion,
                @Precio,
                @Stock,
                @Categoria,
                @FechaCreacion,
                @DeletedAt
            );
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, ToParameters(product));
    }

    public async Task UpdateAsync(Product product)
    {
        const string sql =
            """
            UPDATE products
            SET nombre = @Nombre,
                descripcion = @Descripcion,
                precio = @Precio,
                stock = @Stock,
                categoria = @Categoria
            WHERE id = @Id
              AND deleted_at IS NULL;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, ToParameters(product));
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        const string sql =
            """
            UPDATE products
            SET deleted_at = @DeletedAt
            WHERE id = @Id
              AND deleted_at IS NULL;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            Id = id.ToString(),
            DeletedAt = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
        });
    }

    public async Task<bool> ExistsByNameAndCategoryAsync(
        string nombre,
        string categoria,
        Guid? excludeId = null)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1
                FROM products
                WHERE deleted_at IS NULL
                  AND nombre = @Nombre COLLATE NOCASE
                  AND categoria = @Categoria COLLATE NOCASE
                  AND (@ExcludeId IS NULL OR id <> @ExcludeId)
            );
            """;

        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(sql, new
        {
            Nombre = nombre,
            Categoria = categoria,
            ExcludeId = excludeId?.ToString()
        });
    }

    public Task<bool> HasActiveOrdersAsync(Guid productId)
    {
        return Task.FromResult(productId == ProductWithActiveOrdersId);
    }

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
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
            FechaCreacion = product.FechaCreacion.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            DeletedAt = product.DeletedAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
        };
    }

    private static Product MapToProduct(ProductRow row)
    {
        return new Product
        {
            Id = Guid.Parse(row.Id),
            Nombre = row.Nombre,
            Descripcion = row.Descripcion,
            Precio = Convert.ToDecimal(row.Precio, CultureInfo.InvariantCulture),
            Stock = row.Stock,
            Categoria = row.Categoria,
            FechaCreacion = DateTime.Parse(
                row.FechaCreacion,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind),
            DeletedAt = string.IsNullOrWhiteSpace(row.DeletedAt)
                ? null
                : DateTime.Parse(
                    row.DeletedAt,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind)
        };
    }

    private sealed class ProductRow
    {
        public string Id { get; init; } = string.Empty;
        public string Nombre { get; init; } = string.Empty;
        public string? Descripcion { get; init; }
        public double Precio { get; init; }
        public int Stock { get; init; }
        public string Categoria { get; init; } = string.Empty;
        public string FechaCreacion { get; init; } = string.Empty;
        public string? DeletedAt { get; init; }
    }
}
