using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Products.API.Data;
using Products.API.Data.Repositories;
using Products.API.ExceptionHandlers;
using Products.API.Services;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = CreateSerilogLogger("Products.API", "logs/products-api-.json");
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Products API";
        return Task.CompletedTask;
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductService>();

builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? x.Exception?.Message : x.ErrorMessage)
            .ToList();

        var errorMessage = string.Join("; ", errors);
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = "Los datos del producto son inválidos.";
        }

        var correlationId = context.HttpContext.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? context.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title = "Solicitud inválida",
            status = 400,
            detail = "Hubo un error de validación en los datos enviados.",
            instance = context.HttpContext.Request.Path.Value,
            errorCode = "PRD-002",
            errorMessage = errorMessage,
            correlationId = correlationId
        });
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
}

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
    options.SwaggerEndpoint("/openapi/v1.json", "Products API");
});

app.UseHttpsRedirection();

app.UseExceptionHandler();

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
