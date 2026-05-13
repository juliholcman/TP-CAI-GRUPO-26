using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using Serilog.Formatting.Json;
using Users.API.DTOs.Responses;
using Users.API.ExceptionHandlers;
using Users.API.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "logs/users-api-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(item => item.Value?.Errors.Count > 0)
                .SelectMany(item => item.Value!.Errors.Select(error => error.ErrorMessage))
                .Where(error => !string.IsNullOrWhiteSpace(error))
                .Distinct();

            var correlationId = context.HttpContext.Response.Headers.TryGetValue("X-Correlation-Id", out var header)
                ? header.ToString()
                : context.HttpContext.TraceIdentifier;

            return new BadRequestObjectResult(new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = string.Join("; ", errors.DefaultIfEmpty("Los datos del usuario son inválidos.")),
                Instance = context.HttpContext.Request.Path.Value,
                ErrorCode = "USR-002",
                ErrorMessage = "Los datos del usuario son inválidos.",
                CorrelationId = correlationId
            });
        };
    });
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UserApiExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<UserService>();
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers.TryGetValue("X-Correlation-Id", out var header)
        ? header.ToString()
        : context.TraceIdentifier;

    context.Response.Headers["X-Correlation-Id"] = correlationId;

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Users API");
});

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();
