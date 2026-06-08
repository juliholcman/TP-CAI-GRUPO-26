namespace Cart.API.Exceptions;

public class CartApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string Title { get; }

    public CartApiException(int statusCode, string errorCode, string message, string title) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Title = title;
    }
}
