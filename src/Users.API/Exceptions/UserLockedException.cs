namespace Users.API.Exceptions;

public class UserLockedException : UserApiException
{
    public UserLockedException()
        : base(StatusCodes.Status403Forbidden, "USR-004", "El usuario está bloqueado por demasiados intentos de inicio de sesión fallidos.", "Usuario bloqueado")
    {
    }
}
