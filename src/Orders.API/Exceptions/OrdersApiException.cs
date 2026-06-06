namespace Orders.API.Exceptions;

public class OrdersApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string Title { get; }

    public OrdersApiException(int statusCode, string errorCode, string message, string title) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Title = title;
    }
}
