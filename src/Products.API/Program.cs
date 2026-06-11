using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
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

// ── 1. Serilog (Observability – Logging) ─────────────────────────────────────
Log.Logger = CreateSerilogLogger("Products.API", "logs/products-api-.json");
builder.Host.UseSerilog();

// ── 2. MVC + OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddControllers(options => { })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Intercepta el 400 automático de DataAnnotations y lo formatea con
        // errorCode PRD-002, según el catálogo del TP.
        options.InvalidModelStateResponseFactory = context =>
        {
            var firstError = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Los datos de la solicitud son inválidos.";

            var correlationId = context.HttpContext.Response.Headers.TryGetValue("X-Correlation-Id", out var cid)
                ? cid.ToString()
                : context.HttpContext.Request.Headers.TryGetValue("X-Correlation-Id", out var rcid)
                    ? rcid.ToString()
                    : string.Empty;

            var problem = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "La solicitud contiene datos inválidos o faltantes.",
                instance = context.HttpContext.Request.Path.Value,
                correlationId,
                errorCode = "PRD-002",
                errorMessage = firstError
            };

            return new BadRequestObjectResult(problem);
        };
    });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Products API";
        return Task.CompletedTask;
    });
});

// ── 3. Health Checks (Observability – Health) ────────────────────────────────
builder.Services.AddHealthChecks();

// ── 4. Exception handlers (IExceptionHandler pattern) ───────────────────────
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── 5. Persistence (SQLite + Dapper) ─────────────────────────────────────────
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<ProductRepository>();

// ── 6. Application Services ───────────────────────────────────────────────────
builder.Services.AddScoped<ProductService>();

var app = builder.Build();

// ── 7. Database initialization ────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();
}

// ── 8. Middleware: Correlation ID (Observability – Tracing) ──────────────────
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

// ── 9. Serilog request logging ────────────────────────────────────────────────
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, _, exception) => GetRequestLogLevel(httpContext, exception);
    options.EnrichDiagnosticContext = EnrichFromRequest;
});

// ── 10. OpenAPI / Swagger ──────────────────────────────────────────────────────
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.SwaggerEndpoint("/openapi/v1.json", "Products API");
});

// ── 11. Exception handler middleware ──────────────────────────────────────────
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.MapControllers();

// ── 12. Health endpoints ──────────────────────────────────────────────────────
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
