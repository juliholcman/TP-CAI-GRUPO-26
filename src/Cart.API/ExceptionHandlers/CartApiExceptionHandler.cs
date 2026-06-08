using Microsoft.AspNetCore.Diagnostics;
using Cart.API.DTOs;
using Cart.API.Exceptions;

namespace Cart.API.ExceptionHandlers;

public class CartApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<CartApiExceptionHandler> _logger;

    public CartApiExceptionHandler(ILogger<CartApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not CartApiException ex)
            return false;

        var correlationId = GetCorrelationId(context);
        _logger.LogWarning(
            exception,
            "Cart API domain error {ErrorCode} with correlation id {CorrelationId}",
            ex.ErrorCode,
            correlationId);

        context.Response.StatusCode = ex.StatusCode;

        await context.Response.WriteAsJsonAsync(new ErrorResponse
        {
            Type = GetTypeUri(ex.StatusCode),
            Title = ex.Title,
            Status = ex.StatusCode,
            Detail = ex.Message,
            Instance = context.Request.Path.Value,
            ErrorCode = ex.ErrorCode,
            ErrorMessage = ex.Message,
            CorrelationId = correlationId
        }, cancellationToken);

        return true;
    }

    private static string? GetCorrelationId(HttpContext context)
    {
        return context.Response.Headers.TryGetValue("X-Correlation-Id", out var correlationId)
            ? correlationId.ToString()
            : context.TraceIdentifier;
    }

    private static string GetTypeUri(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
            _ => "https://tools.ietf.org/html/rfc7231"
        };
    }
}
