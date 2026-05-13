# Migracion a microservicios

## Estructura final

```text
ECommerce.sln
src/
  Products.API/
    Controllers/
    Models/
    DTOs/Requests/
    DTOs/Responses/
    Services/
    Exceptions/
    ExceptionHandlers/
    logs/
    Program.cs
    appsettings.json
  Users.API/
    Controllers/
    Models/
    DTOs/Requests/
    DTOs/Responses/
    Services/
    Exceptions/
    ExceptionHandlers/
    logs/
    Program.cs
    appsettings.json
  Orders.API/
    Controllers/
    Models/
    DTOs/Requests/
    DTOs/Responses/
    Services/
    Exceptions/
    ExceptionHandlers/
    logs/
    Program.cs
    appsettings.json
  Cart.API/
    Controllers/
    Models/
    DTOs/Requests/
    DTOs/Responses/
    Services/
    Exceptions/
    ExceptionHandlers/
    logs/
    Program.cs
    appsettings.json
  Notifications.API/
    Controllers/
    Models/
    DTOs/Requests/
    DTOs/Responses/
    Services/
    Exceptions/
    ExceptionHandlers/
    logs/
    Program.cs
    appsettings.json
docs/
README.md
```

## Comandos dotnet CLI

```bash
dotnet new webapi -n Products.API -o src/Products.API --framework net9.0
dotnet new webapi -n Users.API -o src/Users.API --framework net9.0
dotnet new webapi -n Orders.API -o src/Orders.API --framework net9.0
dotnet new webapi -n Cart.API -o src/Cart.API --framework net9.0
dotnet new webapi -n Notifications.API -o src/Notifications.API --framework net9.0

dotnet sln ECommerce.sln add src/Products.API/Products.API.csproj
dotnet sln ECommerce.sln add src/Users.API/Users.API.csproj
dotnet sln ECommerce.sln add src/Orders.API/Orders.API.csproj
dotnet sln ECommerce.sln add src/Cart.API/Cart.API.csproj
dotnet sln ECommerce.sln add src/Notifications.API/Notifications.API.csproj

dotnet restore ECommerce.sln
dotnet build ECommerce.sln

dotnet run --project src/Products.API/Products.API.csproj
dotnet run --project src/Users.API/Users.API.csproj
dotnet run --project src/Orders.API/Orders.API.csproj
dotnet run --project src/Cart.API/Cart.API.csproj
dotnet run --project src/Notifications.API/Notifications.API.csproj
```

## Archivos a mover

El proyecto monolitico original no tenia codigo de negocio separado; solo tenia el endpoint de ejemplo `weatherforecast` en `Program.cs`. Ese endpoint fue eliminado junto con el proyecto monolitico porque no pertenece a ningun dominio del e-commerce.

Cuando existan archivos de dominio, moverlos asi:

```text
Controllers/Products*        -> src/Products.API/Controllers/
Models/Product*              -> src/Products.API/Models/
Services/Product*            -> src/Products.API/Services/
DTOs/**/Product*             -> src/Products.API/DTOs/

Controllers/Users*           -> src/Users.API/Controllers/
Models/User*                 -> src/Users.API/Models/
Services/User*               -> src/Users.API/Services/
DTOs/**/User*                -> src/Users.API/DTOs/

Controllers/Orders*          -> src/Orders.API/Controllers/
Models/Order*                -> src/Orders.API/Models/
Services/Order*              -> src/Orders.API/Services/
DTOs/**/Order*               -> src/Orders.API/DTOs/

Controllers/Cart*            -> src/Cart.API/Controllers/
Models/Cart*                 -> src/Cart.API/Models/
Services/Cart*               -> src/Cart.API/Services/
DTOs/**/Cart*                -> src/Cart.API/DTOs/

Controllers/Notifications*   -> src/Notifications.API/Controllers/
Models/Notification*         -> src/Notifications.API/Models/
Services/Notification*       -> src/Notifications.API/Services/
DTOs/**/Notification*        -> src/Notifications.API/DTOs/
```

Cada servicio conserva su propio `Program.cs`, `appsettings.json` y OpenAPI/Swagger habilitado en `/openapi/v1.json`.
