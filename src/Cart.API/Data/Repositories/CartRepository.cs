using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Cart.API.Models;

namespace Cart.API.Data.Repositories;

public class CartRepository
{
    private readonly string _connectionString;

    public CartRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La connection string 'DefaultConnection' no está configurada.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    // ── Cart existence ────────────────────────────────────────────────────────

    public async Task<bool> CartExistsAsync(Guid userId)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1 FROM carts
                WHERE usuario_id = @UsuarioId
                  AND deleted_at IS NULL
            );
            """;

        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(sql, new { UsuarioId = userId.ToString() });
    }

    public async Task<bool> ItemExistsAsync(Guid userId, Guid productId)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1 FROM cart_items
                WHERE usuario_id  = @UsuarioId
                  AND producto_id = @ProductoId
            );
            """;

        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(sql, new
        {
            UsuarioId  = userId.ToString(),
            ProductoId = productId.ToString()
        });
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public async Task<Cart.API.Models.Cart?> GetByUserIdAsync(Guid userId)
    {
        const string cartSql =
            """
            SELECT usuario_id AS UsuarioId,
                   fecha_actualizacion AS FechaActualizacion
            FROM carts
            WHERE usuario_id = @UsuarioId
              AND deleted_at IS NULL;
            """;

        const string itemsSql =
            """
            SELECT id          AS Id,
                   usuario_id  AS UsuarioId,
                   producto_id AS ProductoId,
                   cantidad    AS Cantidad,
                   nombre      AS Nombre,
                   precio      AS Precio
            FROM cart_items
            WHERE usuario_id = @UsuarioId;
            """;

        await using var connection = await OpenConnectionAsync();

        var cartRow = await connection.QuerySingleOrDefaultAsync<CartRow>(
            cartSql, new { UsuarioId = userId.ToString() });

        if (cartRow is null)
            return null;

        var itemRows = await connection.QueryAsync<CartItemRow>(
            itemsSql, new { UsuarioId = userId.ToString() });

        return MapToCart(cartRow, itemRows);
    }

    // ── Write ─────────────────────────────────────────────────────────────────

    public async Task CreateOrUpdateCartAsync(Guid userId)
    {
        const string sql =
            """
            INSERT INTO carts (usuario_id, fecha_actualizacion, deleted_at)
            VALUES (@UsuarioId, @FechaActualizacion, NULL)
            ON CONFLICT(usuario_id) DO UPDATE
                SET fecha_actualizacion = excluded.fecha_actualizacion,
                    deleted_at          = NULL;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            UsuarioId           = userId.ToString(),
            FechaActualizacion  = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
        });
    }

    public async Task AddOrUpdateItemAsync(Guid userId, CartItem item)
    {
        const string sql =
            """
            INSERT INTO cart_items (id, usuario_id, producto_id, cantidad, nombre, precio)
            VALUES (@Id, @UsuarioId, @ProductoId, @Cantidad, @Nombre, @Precio)
            ON CONFLICT(id) DO UPDATE
                SET cantidad = excluded.cantidad,
                    nombre   = excluded.nombre,
                    precio   = excluded.precio;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            Id         = Guid.NewGuid().ToString(),
            UsuarioId  = userId.ToString(),
            ProductoId = item.ProductId.ToString(),
            item.Cantidad,
            item.Nombre,
            Precio     = Convert.ToDouble(item.Precio, CultureInfo.InvariantCulture)
        });
    }

    public async Task UpdateItemQuantityAsync(Guid userId, Guid productId, int cantidad)
    {
        const string sql =
            """
            UPDATE cart_items
            SET cantidad = @Cantidad
            WHERE usuario_id  = @UsuarioId
              AND producto_id = @ProductoId;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            Cantidad   = cantidad,
            UsuarioId  = userId.ToString(),
            ProductoId = productId.ToString()
        });
    }

    public async Task UpdateItemPriceAndQuantityAsync(Guid userId, Guid productId, int cantidad, decimal precio)
    {
        const string sql =
            """
            UPDATE cart_items
            SET cantidad = @Cantidad,
                precio   = @Precio
            WHERE usuario_id  = @UsuarioId
              AND producto_id = @ProductoId;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            Cantidad   = cantidad,
            Precio     = Convert.ToDouble(precio, CultureInfo.InvariantCulture),
            UsuarioId  = userId.ToString(),
            ProductoId = productId.ToString()
        });
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        const string sql =
            """
            DELETE FROM cart_items
            WHERE usuario_id  = @UsuarioId
              AND producto_id = @ProductoId;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            UsuarioId  = userId.ToString(),
            ProductoId = productId.ToString()
        });
    }

    public async Task ClearCartAsync(Guid userId)
    {
        const string deleteItems =
            "DELETE FROM cart_items WHERE usuario_id = @UsuarioId;";

        const string deleteCart =
            "DELETE FROM carts WHERE usuario_id = @UsuarioId;";

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(deleteItems, new { UsuarioId = userId.ToString() });
        await connection.ExecuteAsync(deleteCart,  new { UsuarioId = userId.ToString() });
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static Cart.API.Models.Cart MapToCart(CartRow cartRow, IEnumerable<CartItemRow> itemRows)
    {
        return new Cart.API.Models.Cart
        {
            UserId = Guid.Parse(cartRow.UsuarioId),
            Items  = itemRows.Select(MapToCartItem).ToList()
        };
    }

    private static CartItem MapToCartItem(CartItemRow row)
    {
        return new CartItem
        {
            ProductId = Guid.Parse(row.ProductoId),
            Nombre    = row.Nombre,
            Precio    = Convert.ToDecimal(row.Precio, CultureInfo.InvariantCulture),
            Cantidad  = row.Cantidad
        };
    }

    // ── Row types ─────────────────────────────────────────────────────────────

    private sealed class CartRow
    {
        public string UsuarioId           { get; init; } = string.Empty;
        public string FechaActualizacion  { get; init; } = string.Empty;
    }

    private sealed class CartItemRow
    {
        public string Id         { get; init; } = string.Empty;
        public string UsuarioId  { get; init; } = string.Empty;
        public string ProductoId { get; init; } = string.Empty;
        public int    Cantidad   { get; init; }
        public string Nombre     { get; init; } = string.Empty;
        public double Precio     { get; init; }
    }
}
