namespace Users.API.Exceptions;

public class FraudBlockedException : UserApiException
{
    public FraudBlockedException()
        : base(StatusCodes.Status403Forbidden, "USR-005", "Usuario bloqueado por detección de fraude.", "Forbidden")
    {
    }
}
