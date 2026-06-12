namespace Users.API.Exceptions;

public class FraudBlockedException : UserApiException
{
    public FraudBlockedException()
        : base(StatusCodes.Status403Forbidden, "USR-005", "El acceso del usuario fue bloqueado por detección de fraude.", "Usuario bloqueado")
    {
    }
}
