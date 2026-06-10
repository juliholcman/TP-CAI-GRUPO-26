namespace Notifications.API.Services;

/// <summary>
/// STUB interno de validación de usuarios.
///
/// DECISIÓN DE DISEÑO:
/// ─────────────────────────────────────────────────────────────────────────────
/// No existe integración real con Users.API en esta iteración del TP.
/// Para cumplir con el contrato NTF-001 (usuario no encontrado → 404) sin
/// aceptar cualquier GUID como válido, se mantiene un set de IDs conocidos
/// que se pre-cargan al iniciar la aplicación.
///
/// Cómo reemplazarlo cuando Users.API esté disponible:
///   1. Crear una clase HttpUserValidator : IUserValidator que llame a
///      GET /api/users/{userId} usando IHttpClientFactory.
///   2. Registrarla en Program.cs en lugar de InMemoryUserValidator.
///   3. No hay que cambiar NotificationService (depende de IUserValidator).
///
/// IDs de usuario válidos para pruebas (véase appsettings.json > UserStub:KnownUsers):
///   - Se cargan desde configuración para facilitar la parametrización en tests.
/// ─────────────────────────────────────────────────────────────────────────────
/// </summary>
public class InMemoryUserValidator : IUserValidator
{
    private readonly HashSet<Guid> _knownUsers;

    public InMemoryUserValidator(IConfiguration configuration)
    {
        // Carga los GUIDs de usuarios conocidos desde configuración.
        // Si la sección no existe, usamos un conjunto mínimo de prueba.
        var raw = configuration.GetSection("UserStub:KnownUsers").Get<string[]>();

        _knownUsers = raw is { Length: > 0 }
            ? raw.Select(Guid.Parse).ToHashSet()
            : new HashSet<Guid>
            {
                // Usuarios de prueba hardcodeados como fallback
                Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Guid.Parse("00000000-0000-0000-0000-000000000002"),
                Guid.Parse("00000000-0000-0000-0000-000000000003")
            };
    }

    /// <inheritdoc />
    public bool UserExists(Guid userId) => _knownUsers.Contains(userId);
}
