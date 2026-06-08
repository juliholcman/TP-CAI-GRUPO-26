namespace Cart.API.Exceptions;

public class NotFoundException : CartApiException
{
    public NotFoundException(string errorCode, string message)
        : base(404, errorCode, message, "Not Found")
    {
    }
}
