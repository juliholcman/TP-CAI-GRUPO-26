# Implementación de Endpoints para Users.API

Se diseñarán e implementarán los endpoints de registro y autenticación para el microservicio `Users.API`, manteniendo la arquitectura limpia ya establecida en el proyecto (ECommerce.sln).

## User Review Required

Por favor, revisa el plan a continuación. Las reglas de negocio (como el límite de 3 intentos fallidos) y los códigos de error (USR-001 a USR-005) están estrictamente adaptados a la solicitud.

> [!IMPORTANT]
> Se utilizará una lista estática en memoria `List<User>` dentro de `UsersService` para simular la persistencia, tal como se hizo en `Products.API`. 
> El manejo del Hash para la contraseña se simulará utilizando `BCrypt.Net` o simplemente concatenando un string de prueba, dado que es una simulación (se propone usar BCrypt si es posible, de lo contrario un cifrado simulado manual para evitar agregar dependencias de terceros si la cátedra no lo permite). **¿Es aceptable usar un hash simulado nativo con SHA256 o prefieres instalar un paquete externo?**

## Proposed Changes

---

### Models y DTOs

#### [NEW] `src/Users.API/Models/User.cs`
- Propiedades: `Id` (Guid), `Nombre` (string), `Apellido` (string), `Email` (string), `PasswordHash` (string), `FechaRegistro` (DateTime), `Activo` (bool), `IntentosFallidos` (int).

#### [NEW] `src/Users.API/DTOs/Requests/RegisterUserRequest.cs`
- Propiedades: `Nombre`, `Apellido`, `Email`, `Password`.
- DataAnnotations: `[Required]`, `[EmailAddress]` para asegurar que USR-002 pueda validarse.

#### [NEW] `src/Users.API/DTOs/Requests/LoginRequest.cs`
- Propiedades: `Email`, `Password`.

#### [NEW] `src/Users.API/DTOs/Responses/UserResponse.cs`
- Propiedades: `Id`, `Nombre`, `Apellido`, `Email`, `FechaRegistro`, `Activo`. (No incluye PasswordHash).

#### [NEW] `src/Users.API/DTOs/Responses/LoginResponse.cs`
- Propiedades: `Id`, `Nombre`, `Apellido`, `Email`. (Se puede unificar con `UserResponse` o dejarlo específico para el login).

---

### Excepciones y Handlers

#### [NEW] `src/Users.API/Exceptions/...`
Creación de las siguientes excepciones personalizadas para mapear a los códigos de error:
- `ConflictException.cs` (409) -> Para USR-001 (Email registrado)
- `ValidationException.cs` (400) -> Para USR-002 (Datos inválidos)
- `UnauthorizedException.cs` (401) -> Para USR-003 (Credenciales incorrectas)
- `ForbiddenException.cs` (403) -> Para USR-004 y USR-005 (Cuenta bloqueada/suspendida)

#### [NEW] `src/Users.API/ExceptionHandlers/...`
Creación de los manejadores que implementan `IExceptionHandler` para formatear a `ProblemDetails`:
- `ConflictExceptionHandler.cs`
- `ValidationExceptionHandler.cs`
- `UnauthorizedExceptionHandler.cs`
- `ForbiddenExceptionHandler.cs`
- `GlobalExceptionHandler.cs` (Para el 500)

---

### Service Layer

#### [NEW] `src/Users.API/Services/UsersService.cs`
- `Register(RegisterUserRequest request)`: Verifica si el email existe (lanza `ConflictException` USR-001). Valida campos (lanza `ValidationException` USR-002). Genera el hash de la contraseña y crea el usuario. Devuelve `UserResponse`.
- `Login(LoginRequest request)`: 
  - Si no existe el usuario, o falla la clave: Incrementa `IntentosFallidos`. Si llega a 3, desactiva la cuenta y lanza `ForbiddenException` USR-004. Si no llega a 3, lanza `UnauthorizedException` USR-003.
  - Si está desactivado (`Activo == false`): lanza `ForbiddenException` USR-004 o USR-005.
  - Si éxito: Resetea `IntentosFallidos = 0`. Devuelve `LoginResponse`.

---

### Controller y Program.cs

#### [NEW] `src/Users.API/Controllers/UsersController.cs`
- `[HttpPost("register")]`: `ActionResult<UserResponse> Register([FromBody] RegisterUserRequest request)`
- `[HttpPost("login")]`: `ActionResult<LoginResponse> Login([FromBody] LoginRequest request)`

#### [MODIFY] `src/Users.API/Program.cs`
- Registrar el `UsersService`.
- Registrar todos los `ExceptionHandlers` (`builder.Services.AddExceptionHandler<...>`).
- Agregar `AddProblemDetails()`.
- Agregar configuración de Swagger.

## Verification Plan

### Automated Tests
- Compilar la solución usando `dotnet build`.

### Manual Verification
- Levantar el proyecto `Users.API`.
- Registrar un usuario correctamente y verificar la ausencia de `PasswordHash` en la respuesta (201).
- Tratar de registrar un usuario con un email ya registrado (409, USR-001).
- Tratar de hacer login con credenciales incorrectas 3 veces seguidas y verificar el bloqueo por intentos fallidos (403, USR-004).
- Comprobar que el response devuelve ProblemDetails con el RFC correspondiente.
