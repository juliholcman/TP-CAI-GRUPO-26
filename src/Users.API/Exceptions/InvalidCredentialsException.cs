namespace Users.API.Exceptions;

public class InvalidCredentialsException : UserApiException
{
    public InvalidCredentialsException()
        : base(StatusCodes.Status401Unauthorized, "USR-003", "Credenciales incorrectas.", "Unauthorized")
    {
    }
}
