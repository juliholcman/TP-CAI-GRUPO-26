namespace Users.API.Exceptions;

public class DuplicateEmailException : UserApiException
{
    public DuplicateEmailException()
        : base(StatusCodes.Status409Conflict, "USR-001", "El correo electrónico ya está registrado.", "Correo electrónico duplicado")
    {
    }
}
