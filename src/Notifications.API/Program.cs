using Microsoft.AspNetCore.Mvc;
using Notifications.API.ExceptionHandlers;
using Notifications.API.Services;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Serilog (Observability - Logging)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/notifications-.json", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 2. Setup HealthChecks (Observability - Health)
builder.Services.AddHealthChecks();

// Registrar la configuración de excepciones de la cátedra
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Registrar nuestro servicio
builder.Services.AddSingleton<NotificationService>();

var app = builder.Build();

// 3. Middleware: Correlation ID (Observability - Tracing)
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        await next();
    }
});

// Configurar Swagger UI
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Notifications API");
});

app.UseExceptionHandler(); // Habilita los IExceptionHandler que agregaste

app.UseHttpsRedirection();
app.MapControllers();

// 4. Mapear HealthChecks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();
