namespace Users.API.Exceptions;

public class UserValidationException : UserApiException
{
    public UserValidationException(string message = "Los datos del usuario son inválidos.")
        : base(StatusCodes.Status400BadRequest, "USR-002", message, "Bad Request")
    {
    }
}
