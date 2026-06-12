# Diagrama de arquitectura

Este diagrama representa la arquitectura de microservicios del TP CAI E-Commerce.

Cada microservicio expone su propia REST API, tiene documentación Swagger/OpenAPI, health checks, logs con Serilog, correlation ID y persistencia propia en SQLite mediante Dapper.

Las comunicaciones entre servicios se realizan por HTTP.

---

## Diagrama

![Diagrama de arquitectura de microservicios](./diagrama-cai.png)

[Abrir diagrama editable en diagrams.net](./diagrama-cai.drawio)

---

## Microservicios

| Servicio             | Base de datos      | Puerto (dev) |
|----------------------|--------------------|--------------|
| Users.API            | users.db           | 5001         |
| Products.API         | products.db        | 5002         |
| Orders.API           | orders.db          | 5003         |
| Cart.API             | cart.db            | 5004         |
| Notifications.API    | notifications.db   | 5005         |

---

## Comunicación HTTP entre servicios

| Origen              | Destino          | Propósito                            |
|---------------------|------------------|--------------------------------------|
| Cart.API            | Products.API     | Valida producto y stock              |
| Orders.API          | Users.API        | Valida usuario                       |
| Orders.API          | Products.API     | Valida productos, stock y precio     |
| Notifications.API   | Users.API        | Valida usuario destinatario          |

> **Nota:** Orders.API no llama a Notifications.API en la implementación actual. La flecha no está incluida en el diagrama.

---

## Tecnologías transversales

- **.NET 9 Web API** — framework base de cada microservicio
- **SQLite + Dapper** — persistencia liviana e independiente por servicio
- **Swagger / OpenAPI** — documentación interactiva de endpoints
- **Health Checks** — `/health`, `/health/ready`, `/health/live`
- **Serilog** — logs estructurados en consola y archivo JSON
- **Correlation ID** — trazabilidad `X-Correlation-Id` por request

---

## Cómo exportar el PNG manualmente

Si necesitás regenerar el PNG desde el archivo `.drawio`:

1. Abrí [diagrams.net](https://app.diagrams.net/) en el navegador.
2. Seleccioná **File → Open from → Device** y cargá `diagrama-cai.drawio`.
3. Una vez cargado, ir a **File → Export as → PNG**.
4. Configurar: **Scale 200%**, fondo blanco, incluir todo el diagrama.
5. Guardarlo como `diagrama-cai.png` en esta misma carpeta.
