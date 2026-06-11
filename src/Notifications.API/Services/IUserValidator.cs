namespace Notifications.API.Services;

/// <summary>
/// Contrato para validar si un usuario existe.
/// En producción esta interfaz puede resolverse contra Users.API vía HttpClient.
/// </summary>
public interface IUserValidator
{
    /// <summary>
    /// Retorna true si el usuario con el id dado existe y está activo.
    /// </summary>
    bool UserExists(Guid userId);
}
