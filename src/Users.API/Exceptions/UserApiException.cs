namespace Users.API.Exceptions;

public class UserApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string Title { get; }

    public UserApiException(int statusCode, string errorCode, string message, string title) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Title = title;
    }
}
