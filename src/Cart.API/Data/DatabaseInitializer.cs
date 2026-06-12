using Dapper;
using Microsoft.Data.Sqlite;

namespace Cart.API.Data;

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
            CREATE TABLE IF NOT EXISTS carts (
                usuario_id       TEXT PRIMARY KEY,
                fecha_actualizacion TEXT NOT NULL
            );
            """);

        connection.Execute(
            """
            CREATE TABLE IF NOT EXISTS cart_items (
                id          TEXT PRIMARY KEY,
                usuario_id  TEXT NOT NULL,
                producto_id TEXT NOT NULL,
                cantidad    INTEGER NOT NULL,
                nombre      TEXT NOT NULL,
                precio      REAL NOT NULL,
                FOREIGN KEY (usuario_id) REFERENCES carts(usuario_id)
            );
            """);
    }
}
