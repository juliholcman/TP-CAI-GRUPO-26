using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

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
            CREATE TABLE IF NOT EXISTS notifications (
                id TEXT PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                mensaje TEXT NOT NULL,
                tipo TEXT NOT NULL,
                estado TEXT NOT NULL,
                fecha_envio TEXT NOT NULL
            );
            """);
    }
}
