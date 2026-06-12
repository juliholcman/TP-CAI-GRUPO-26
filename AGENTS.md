## TP-specific rules

This project must follow the assignment contract exactly.

- Build each API as a .NET 9 REST API.
- Use the persistence library provided by the course only from the Services layer.
- Do not introduce EF Core, DbContext, migrations, or custom persistence unless explicitly requested.

## Error contract

All 4xx and 5xx responses must follow the assignment ProblemDetails-like contract:

- type
- title
- status
- detail
- instance
- errorCode
- errorMessage

Use the exact errorCode catalog for each API:
- PRD-* for Products
- USR-* for Users
- ORD-* for Orders
- CRT-* for Cart
- NTF-* for Notifications

Do not return ad-hoc error shapes.

## Exception handling

Use .NET 9 IExceptionHandler.

Required:
- register handlers with builder.Services.AddExceptionHandler<...>()
- call builder.Services.AddProblemDetails()
- call app.UseExceptionHandler()

Do not create custom error middleware for this.

Controllers should not catch business exceptions.
Services should throw domain exceptions with errorCode and message.

## Swagger / OpenAPI

Every endpoint must document:
- request body when applicable
- success response
- every possible error status
- errorCode/errorMessage examples

Use XML comments and ProducesResponseType when possible.

## Observability

Each service must include:
- Serilog console sink
- Serilog file sink with structured JSON logs
- X-Correlation-Id per request
- Correlation ID included in logs
- Correlation ID included in error responses
- /health
- /health/ready
- /health/live
