using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Data.Repositories;

public class NotificationRepository
{
    private const string NotificationColumns =
        """
        id AS Id,
        usuario_id AS UsuarioId,
        mensaje AS Mensaje,
        tipo AS Tipo,
        estado AS Estado,
        fecha_envio AS FechaEnvio
        """;

    private readonly string _connectionString;

    public NotificationRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("La connection string 'DefaultConnection' no está configurada.");
    }

    /// <summary>
    /// Obtiene todas las notificaciones de un usuario (excluye soft-deleted si aplica).
    /// </summary>
    public async Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(Guid userId)
    {
        var sql = $"SELECT {NotificationColumns} FROM notifications WHERE usuario_id = @UsuarioId AND deleted_at IS NULL ORDER BY fecha_envio;";

        await using var connection = await OpenConnectionAsync();
        var rows = await connection.QueryAsync<NotificationRow>(sql, new { UsuarioId = userId.ToString() });

        return rows.Select(MapToNotification).ToArray();
    }

    /// <summary>
    /// Persiste una nueva notificación.
    /// </summary>
    public async Task CreateAsync(Notification notification)
    {
        const string sql =
            """
            INSERT INTO notifications (
                id,
                usuario_id,
                mensaje,
                tipo,
                estado,
                fecha_envio,
                deleted_at
            )
            VALUES (
                @Id,
                @UsuarioId,
                @Mensaje,
                @Tipo,
                @Estado,
                @FechaEnvio,
                @DeletedAt
            );
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, ToParameters(notification));
    }

    /// <summary>
    /// Actualiza el estado de una notificación existente.
    /// </summary>
    public async Task UpdateStatusAsync(Guid id, string estado)
    {
        const string sql =
            """
            UPDATE notifications
            SET estado = @Estado
            WHERE id = @Id
              AND deleted_at IS NULL;
            """;

        await using var connection = await OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new { Id = id.ToString(), Estado = estado });
    }

    /// <summary>
    /// Indica si existe al menos una notificación para el usuario dado.
    /// </summary>
    public async Task<bool> ExistsForUserAsync(Guid userId)
    {
        const string sql =
            """
            SELECT EXISTS (
                SELECT 1
                FROM notifications
                WHERE usuario_id = @UsuarioId
                  AND deleted_at IS NULL
            );
            """;

        await using var connection = await OpenConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(sql, new { UsuarioId = userId.ToString() });
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private async Task<SqliteConnection> OpenConnectionAsync()
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static object ToParameters(Notification notification)
    {
        return new
        {
            Id = notification.Id.ToString(),
            UsuarioId = notification.UsuarioId.ToString(),
            notification.Mensaje,
            notification.Tipo,
            notification.Estado,
            FechaEnvio = notification.FechaEnvio.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            DeletedAt = (string?)null
        };
    }

    private static Notification MapToNotification(NotificationRow row)
    {
        return new Notification
        {
            Id = Guid.Parse(row.Id),
            UsuarioId = Guid.Parse(row.UsuarioId),
            Mensaje = row.Mensaje,
            Tipo = row.Tipo,
            Estado = row.Estado,
            FechaEnvio = DateTime.Parse(
                row.FechaEnvio,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind)
        };
    }

    private sealed class NotificationRow
    {
        public string Id { get; init; } = string.Empty;
        public string UsuarioId { get; init; } = string.Empty;
        public string Mensaje { get; init; } = string.Empty;
        public string Tipo { get; init; } = string.Empty;
        public string Estado { get; init; } = string.Empty;
        public string FechaEnvio { get; init; } = string.Empty;
    }
}
