using Serilog;
using Serilog.Context;
using Serilog.Formatting.Json;
using Cart.API.ExceptionHandlers;
using Cart.API.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Serilog (Observability - Logging)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(), "logs/cart-api-.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add controllers and standard OpenAPI services
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 3. Register ProblemDetails and custom Exception Handlers (Exception handling)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<CartApiExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();

// 4. Register Health Checks (Observability - Health endpoints)
builder.Services.AddHealthChecks();

// 5. Register HttpClient for Products API Integration
var productsApiUrl = builder.Configuration["ProductsApi:BaseUrl"] ?? "https://localhost:61008";
builder.Services.AddHttpClient<CartService>(client =>
{
    client.BaseAddress = new Uri(productsApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// 6. Correlation ID Middleware (Observability - Correlation ID)
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
    options.SwaggerEndpoint("/openapi/v1.json", "Cart API");
});

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();

// 7. Health endpoints (Observability - Health Checks)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();

