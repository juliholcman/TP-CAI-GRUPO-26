## Descripción

Corrige validaciones y contrato de errores de Notifications.API según el contrato del TP CAI (PDF 1).

## Cambios

### Validaciones de entrada
- `NotificationRequest`: agrega `[Required]` en `UsuarioId`, `Mensaje` y `Tipo`; `[MaxLength(500)]` en `Mensaje`
- `InvalidModelStateResponseFactory` en `Program.cs`: formatea errores de DataAnnotations como **NTF-002** con `correlationId`
- Servicio valida `Guid.Empty` → NTF-002
- Servicio valida `Tipo` contra whitelist `{ Email, Push, SMS }` → NTF-002

### NTF-001: usuario inexistente
- Nueva interfaz `IUserValidator` y stub `InMemoryUserValidator` (GUIDs cargados desde `appsettings.json > UserStub:KnownUsers`)
- Diseñado para reemplazarse por `HttpUserValidator` cuando Users.API esté disponible sin tocar `NotificationService`

### Corrección de catálogo
- `GlobalExceptionHandler`: `NTF-500` → **NTF-004**

### Observabilidad
- `correlationId` incluido en el cuerpo de todos los errores (400, 404, 500)

### Swagger
- `POST /api/notifications/send`: documenta 201, 400 (NTF-002), 404 (NTF-001), 500 (NTF-004)
- `GET /api/notifications/{userId}`: documenta 200, 404 (NTF-003), **500 (NTF-004)** (faltaba)
- XML comments con descripción de cada errorCode

## Criterios de aceptación

| Caso | Resultado esperado |
|---|---|
| POST válido | 201 |
| POST Tipo inválido | 400 NTF-002 |
| POST Mensaje vacío | 400 NTF-002 |
| POST UsuarioId inexistente | 404 NTF-001 |
| POST UsuarioId = Guid.Empty | 400 NTF-002 |
| GET usuario sin notificaciones | 404 NTF-003 |
| Error interno | 500 NTF-004 |

## Archivos nuevos
- `Services/IUserValidator.cs`
- `Services/InMemoryUserValidator.cs`
