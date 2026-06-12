# ECommerce

Solucion migrada desde un WebAPI monolitico hacia cinco microservicios WebAPI independientes en .NET 9.

## Estructura

```text
ECommerce.sln
src/
  Products.API/
  Users.API/
  Orders.API/
  Cart.API/
  Notifications.API/
docs/
README.md
```

Cada microservicio contiene:

```text
Controllers/
Models/
DTOs/
  Requests/
  Responses/
Services/
Exceptions/
ExceptionHandlers/
logs/
Program.cs
appsettings.json
appsettings.Development.json
```

## Uso local

El proyecto apunta a .NET 9. Si no tenes el SDK instalado en macOS:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet" --architecture arm64
```

Comandos utiles:

```bash
./scripts/project.sh restore
./scripts/project.sh build
./scripts/project.sh run Products.API
./scripts/project.sh run Users.API
./scripts/project.sh run Orders.API
./scripts/project.sh run Cart.API
./scripts/project.sh run Notifications.API
```

Tambien se puede usar `dotnet` directamente:

```bash
dotnet restore ECommerce.sln
dotnet build ECommerce.sln
dotnet run --project src/Products.API/Products.API.csproj
dotnet run --project src/Users.API/Users.API.csproj
dotnet run --project src/Orders.API/Orders.API.csproj
dotnet run --project src/Cart.API/Cart.API.csproj
dotnet run --project src/Notifications.API/Notifications.API.csproj
```

## Migracion

El WebAPI monolitico original solo contenia el endpoint de ejemplo `weatherforecast`; no habia codigo de dominio para mover. Por eso la migracion realizada es estructural y no agrega logica de negocio nueva.

Mapa previsto para futuros archivos de dominio:

```text
Productos       -> src/Products.API/
Usuarios        -> src/Users.API/
Ordenes         -> src/Orders.API/
Carrito         -> src/Cart.API/
Notificaciones  -> src/Notifications.API/
```

## Arquitectura

La solución adopta una arquitectura de **microservicios independientes**: cada servicio tiene su propia base de datos SQLite, expone una REST API en .NET 9 y se comunica con otros servicios exclusivamente por HTTP.

```
╔══════════════════════════════════════════════════════════════════════════════════════════╗
║            Arquitectura de Microservicios — TP CAI E-Commerce                           ║
╚══════════════════════════════════════════════════════════════════════════════════════════╝

 Comunicaciones HTTP entre microservicios (··> = llamada HTTP):
 ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄
 Notifications.API ·····················································> Users.API
                                                     Valida usuario destinatario

 Cart.API ··········································> Products.API
                                  Valida producto y stock

 Orders.API ···················> Users.API
                 Valida usuario

 Orders.API ········> Products.API
     Valida productos, stock y precio
 ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄

 Microservicios y persistencia propia (-> = guarda/lee en su base de datos):

 ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌───────────┐  ┌────────────────────┐
 │  Users.API  │  │ Products.API │  │ Orders.API  │  │ Cart.API  │  │  Notifications.API │
 └──────┬──────┘  └──────┬───────┘  └──────┬──────┘  └─────┬─────┘  └─────────┬──────────┘
        │                │                  │               │                   │
        ▼                ▼                  ▼               ▼                   ▼
   ┌──────────┐  ┌─────────────┐  ┌────────────┐  ┌──────────┐  ┌──────────────────┐
   │ users.db │  │ products.db │  │ orders.db  │  │ cart.db  │  │ notifications.db │
   └──────────┘  └─────────────┘  └────────────┘  └──────────┘  └──────────────────┘

 Nota: Orders.API no llama a Notifications.API en la implementacion actual.

 Tecnologias transversales:
 .NET Web API · SQLite + Dapper · Swagger/OpenAPI · Health Checks · Serilog · Correlation ID
```
