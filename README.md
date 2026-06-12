# TP CAI - Microservicios

## Grupo 26

**Integrantes:**

- Julieta Holcman
- Tomás Altman
- Sol Beraja

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
Data/
logs/
Program.cs
appsettings.json
appsettings.Development.json
```

## Rama de entrega

La versión final del TP se encuentra en la rama:

```bash
main
```

Todas las funcionalidades, documentación y evidencias de prueba fueron consolidadas en esa rama.

## Uso local

El proyecto apunta a .NET 9. Si no tenés el SDK instalado en macOS:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
bash /tmp/dotnet-install.sh --channel 9.0 --install-dir "$HOME/.dotnet" --architecture arm64
```

Comandos útiles:

```bash
./project.sh restore
./project.sh build
./project.sh run Products.API
./project.sh run Users.API
./project.sh run Orders.API
./project.sh run Cart.API
./project.sh run Notifications.API
```

## Aclaración sobre versión de .NET

El enunciado principal del TP indicaba el uso de .NET 8. En nuestro caso, el proyecto fue desarrollado con .NET 9 por una decisión práctica de entorno: parte del equipo trabajó en macOS y resultó más simple mantener un entorno estable entre macOS y Windows usando .NET 9.

De todos modos, se respetó la arquitectura solicitada por el TP: microservicios independientes en ASP.NET Core, Swagger/OpenAPI, Health Checks, Serilog, Correlation ID y persistencia con SQLite + Dapper.

## Correr todas las APIs

```bash
./project.sh
```

Este comando compila la solución y levanta las cinco APIs independientes en conjunto para facilitar el testing y las demos. No las unifica en una sola API. Presioná `Ctrl+C` para detener todos los procesos.

También se puede usar `dotnet` directamente:

```bash
dotnet restore ECommerce.sln
dotnet build ECommerce.sln
dotnet run --project src/Products.API/Products.API.csproj
dotnet run --project src/Users.API/Users.API.csproj
dotnet run --project src/Orders.API/Orders.API.csproj
dotnet run --project src/Cart.API/Cart.API.csproj
dotnet run --project src/Notifications.API/Notifications.API.csproj
```

## Pruebas y evidencia

Para facilitar la validación del TP, automatizamos parte de las pruebas usando Node.js dentro de la carpeta `qa-automation/`.

La automatización permite ejecutar pruebas por API y generar capturas de evidencia desde Swagger. Las capturas quedan organizadas por microservicio dentro de `docs/`.

Ejemplos de uso:

```bash
cd qa-automation
npm install
npm run qa -- products
npm run qa -- users
npm run qa -- cart
npm run qa -- notifications
npm run qa -- orders
```

Estas pruebas automatizadas se usaron para acelerar la generación de evidencia, pero luego fueron revisadas manualmente para validar que:

- los endpoints respondan con los códigos HTTP esperados;
- los errores incluyan `errorCode` y `errorMessage`;
- no se expongan datos internos como `PasswordHash` o `DeletedAt`;
- Swagger muestre correctamente requests y responses;
- la evidencia generada sea clara para la entrega.

Las capturas se organizaron por API:

```text
docs/
  products/
    screenshots/
    errors/
  users/
    screenshots/
    errors/
  cart/
    screenshots/
    errors/
  orders/
    screenshots/
    errors/
  notifications/
    screenshots/
    errors/
```

## Herramientas de apoyo utilizadas

Durante el desarrollo utilizamos herramientas de asistencia como Antigravity y Codex para acelerar tareas de implementación, revisión, documentación y generación de evidencia.

Estas herramientas fueron usadas como apoyo técnico. El equipo revisó manualmente el código, los endpoints, las respuestas de error, las capturas y el funcionamiento general del sistema antes de la entrega.

## Arquitectura

La solución adopta una arquitectura de **microservicios independientes**: cada servicio tiene su propia base de datos SQLite, expone una REST API en .NET 9 y se comunica con otros servicios exclusivamente por HTTP.


### Comunicaciones HTTP entre servicios

| Origen              | Destino          | Propósito                            |
|---------------------|------------------|--------------------------------------|
| Cart.API            | Products.API     | Valida producto y stock              |
| Orders.API          | Users.API        | Valida usuario                       |
| Orders.API          | Products.API     | Valida productos, stock y precio     |
| Notifications.API   | Users.API        | Valida usuario destinatario          |

**Tecnologías transversales:** .NET Web API · SQLite + Dapper · Swagger/OpenAPI · Health Checks · Serilog · Correlation ID