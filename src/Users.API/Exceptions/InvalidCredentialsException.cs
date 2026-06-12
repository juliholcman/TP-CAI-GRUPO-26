namespace Users.API.Exceptions;

public class InvalidCredentialsException : UserApiException
{
    public InvalidCredentialsException()
        : base(StatusCodes.Status401Unauthorized, "USR-003", "El correo electrónico o la contraseña son incorrectos.", "Credenciales inválidas")
    {
    }
}
