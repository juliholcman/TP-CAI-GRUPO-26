using Microsoft.AspNetCore.Mvc;
using Products.API.ExceptionHandlers;
using Products.API.Services;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Serilog (Observability – Logging) ─────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(
        new Serilog.Formatting.Json.JsonFormatter(),
        "logs/products-.json",
        rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("ServiceName", "Products.API")
    .CreateLogger();
builder.Host.UseSerilog();

// ── 2. MVC + OpenAPI ──────────────────────────────────────────────────────────
builder.Services.AddControllers()
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

            var correlationId = context.HttpContext.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                             ?? context.HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                             ?? string.Empty;

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

builder.Services.AddOpenApi();

// ── 3. Health Checks (Observability – Health) ────────────────────────────────
builder.Services.AddHealthChecks();

// ── 4. Exception handlers ────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ConflictExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── 5. Servicios de aplicación ────────────────────────────────────────────────
builder.Services.AddSingleton<ProductService>();

var app = builder.Build();

// ── 6. Middleware: Correlation ID (Observability – Tracing) ──────────────────
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                     ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

// ── 7. Swagger UI ─────────────────────────────────────────────────────────────
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Products API");
});

// ── 8. Exception handler middleware ──────────────────────────────────────────
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.MapControllers();

// ── 9. Health endpoints ───────────────────────────────────────────────────────
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();
