namespace Users.API.Exceptions;

public class UserLockedException : UserApiException
{
    public UserLockedException()
        : base(StatusCodes.Status403Forbidden, "USR-004", "Usuario bloqueado por demasiados intentos fallidos.", "Forbidden")
    {
    }
}
