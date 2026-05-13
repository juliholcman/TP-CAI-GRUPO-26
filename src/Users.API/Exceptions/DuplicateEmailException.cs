namespace Users.API.Exceptions;

public class DuplicateEmailException : UserApiException
{
    public DuplicateEmailException()
        : base(StatusCodes.Status409Conflict, "USR-001", "El email ya está registrado.", "Conflict")
    {
    }
}
