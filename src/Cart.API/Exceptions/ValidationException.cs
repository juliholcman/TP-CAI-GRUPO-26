namespace Cart.API.Exceptions;

public class ValidationException : CartApiException
{
    public ValidationException(string errorCode, string message)
        : base(400, errorCode, message, "Bad Request")
    {
    }
}
