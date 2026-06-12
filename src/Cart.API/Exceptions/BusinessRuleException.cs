namespace Cart.API.Exceptions;

public class BusinessRuleException : CartApiException
{
    public BusinessRuleException(string errorCode, string message)
        : base(422, errorCode, message, "Regla de negocio no satisfecha")
    {
    }
}
