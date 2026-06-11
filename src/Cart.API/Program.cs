using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;
using Cart.API.ExceptionHandlers;
using Cart.API.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = CreateSerilogLogger("Cart.API", "logs/cart-api-.json");
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Cart API";
        return Task.CompletedTask;
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CartApiExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
builder.Services.AddHealthChecks();

var productsApiUrl = builder.Configuration["ProductsApi:BaseUrl"] ?? "https://localhost:61008";
builder.Services.AddHttpClient<CartService>(client =>
{
    client.BaseAddress = new Uri(productsApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
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

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, _, exception) => GetRequestLogLevel(httpContext, exception);
    options.EnrichDiagnosticContext = EnrichFromRequest;
});

app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/openapi/v1.json", "Cart API");
});

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health", CreateHealthCheckOptions());
app.MapHealthChecks("/health/ready", CreateHealthCheckOptions(check => check.Tags.Contains("ready")));
app.MapHealthChecks("/health/live", CreateHealthCheckOptions(_ => false));

app.Run();

static Serilog.ILogger CreateSerilogLogger(string serviceName, string jsonLogPath)
{
    return new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Service", serviceName)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(new JsonFormatter(), jsonLogPath, rollingInterval: RollingInterval.Day)
        .CreateLogger();
}

static LogEventLevel GetRequestLogLevel(HttpContext httpContext, Exception? exception)
{
    if (IsNoisyPath(httpContext.Request.Path))
        return LogEventLevel.Verbose;

    return exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
        ? LogEventLevel.Error
        : LogEventLevel.Information;
}

static bool IsNoisyPath(PathString path)
{
    return path.StartsWithSegments("/health")
        || path.StartsWithSegments("/swagger")
        || path.StartsWithSegments("/openapi");
}

static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
{
    var correlationId = httpContext.Response.Headers.TryGetValue("X-Correlation-Id", out var header)
        ? header.ToString()
        : httpContext.TraceIdentifier;

    diagnosticContext.Set("CorrelationId", correlationId);
    diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
    diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? string.Empty);
    diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
}

static HealthCheckOptions CreateHealthCheckOptions(Func<HealthCheckRegistration, bool>? predicate = null)
{
    var options = new HealthCheckOptions
    {
        ResponseWriter = WriteHealthCheckResponseAsync
    };

    if (predicate is not null)
        options.Predicate = predicate;

    return options;
}

static Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = JsonSerializer.Serialize(new
    {
        status = report.Status.ToString()
    });

    return context.Response.WriteAsync(response);
}
