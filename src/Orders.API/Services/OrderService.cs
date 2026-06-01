using Orders.API.DTOs.Requests;
using Orders.API.DTOs.Responses;
using Orders.API.Exceptions;
using Orders.API.Models;

namespace Orders.API.Services;

public class OrderService
{
    private class SimulatedUser
    {
        public Guid Id { get; set; }
    }

    private class SimulatedProduct
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    // Datos simulados de usuarios (enlace con Users.API)
    private readonly List<SimulatedUser> _usersDb = new()
    {
        new SimulatedUser { Id = Guid.Parse("a1b2c3d4-0000-0000-0000-111122223333") } // María
    };

    // Datos simulados de productos (enlace con Products.API)
    private readonly List<SimulatedProduct> _productsDb = new()
    {
        new SimulatedProduct
        {
            Id = Guid.Parse("b69b109d-9c5c-4f68-9942-a0ba2f4710b1"),
            Nombre = "Notebook Lenovo IdeaPad",
            Precio = 899999.99m,
            Stock = 12
        },
        new SimulatedProduct
        {
            Id = Guid.Parse("3b1c1b9f-5c49-4944-b6ce-d6edc40a42a7"),
            Nombre = "Mouse Logitech M280",
            Precio = 24999.50m,
            Stock = 35
        },
        new SimulatedProduct
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            Nombre = "Notebook Dell XPS 15",
            Precio = 1500.00m,
            Stock = 2
        }
    };

    private readonly List<Order> _orders = new()
    {
        new Order
        {
            Id = Guid.Parse("f1e2d3c4-0000-0000-0000-aabbccddeeff"),
            UsuarioId = Guid.Parse("a1b2c3d4-0000-0000-0000-111122223333"),
            Items = new[]
            {
                new OrderItem
                {
                    ProductoId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    Cantidad = 2,
                    PrecioUnitario = 1500.00m
                }
            },
            Estado = "Pendiente",
            FechaCreacion = new DateTime(2024, 3, 10, 11, 0, 0, DateTimeKind.Utc)
        },
        new Order
        {
            Id = Guid.Parse("f1e2d3c4-0000-0000-0000-111122223333"),
            UsuarioId = Guid.Parse("a1b2c3d4-0000-0000-0000-111122223333"),
            Items = new[]
            {
                new OrderItem
                {
                    ProductoId = Guid.Parse("3b1c1b9f-5c49-4944-b6ce-d6edc40a42a7"),
                    Cantidad = 1,
                    PrecioUnitario = 24999.50m
                }
            },
            Estado = "Confirmada",
            FechaCreacion = new DateTime(2026, 2, 15, 15, 30, 0, DateTimeKind.Utc)
        },
        new Order
        {
            Id = Guid.Parse("f1e2d3c4-0000-0000-0000-444455556666"),
            UsuarioId = Guid.Parse("c9a8b7a6-5555-4444-3333-222211110000"),
            Items = new[]
            {
                new OrderItem
                {
                    ProductoId = Guid.Parse("b69b109d-9c5c-4f68-9942-a0ba2f4710b1"),
                    Cantidad = 1,
                    PrecioUnitario = 899999.99m
                }
            },
            Estado = "Enviada",
            FechaCreacion = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc)
        }
    };

    private readonly object _syncRoot = new();

    public IEnumerable<OrderResponse> GetOrders(Guid? usuarioId)
    {
        lock (_syncRoot)
        {
            // Simular un error interno si se pasa el UUID mágico de prueba para 500 Internal Server Error
            if (usuarioId == Guid.Parse("99999999-9999-9999-9999-999999999999"))
            {
                throw new InvalidOperationException("Error inesperado en la persistencia de datos (Simulado).");
            }

            var query = _orders.AsEnumerable();

            if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
            {
                query = query.Where(o => o.UsuarioId == usuarioId.Value);
            }

            return query.Select(o => new OrderResponse
            {
                Id = o.Id,
                UsuarioId = o.UsuarioId,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList(),
                Total = o.Total,
                Estado = o.Estado,
                FechaCreacion = o.FechaCreacion
            }).ToList();
        }
    }

    public OrderResponse GetOrderById(Guid id)
    {
        lock (_syncRoot)
        {
            // Simular un error interno si se pasa el UUID mágico de prueba para 500 Internal Server Error
            if (id == Guid.Parse("99999999-9999-9999-9999-999999999999"))
            {
                throw new InvalidOperationException("Error inesperado en la persistencia de datos al buscar por ID (Simulado).");
            }

            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
            {
                throw new OrdersNotFoundException("ORD-001", "Orden no encontrada.");
            }

            return new OrderResponse
            {
                Id = order.Id,
                UsuarioId = order.UsuarioId,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList(),
                Total = order.Total,
                Estado = order.Estado,
                FechaCreacion = order.FechaCreacion
            };
        }
    }

    public OrderResponse CreateOrder(CreateOrderRequest request)
    {
        lock (_syncRoot)
        {
            // Validaciones de negocio manuales para robustez
            if (request == null)
            {
                throw new OrdersValidationException("Los datos de la orden son inválidos.");
            }
            if (request.UsuarioId == Guid.Empty)
            {
                throw new OrdersValidationException("El ID de usuario es obligatorio.");
            }
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new OrdersValidationException("La lista de ítems no puede estar vacía.");
            }

            // Verificar si el usuario existe (ORD-003)
            var userExists = _usersDb.Any(u => u.Id == request.UsuarioId);
            if (!userExists)
            {
                throw new UserNotFoundException();
            }

            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                if (item.ProductoId == Guid.Empty)
                {
                    throw new OrdersValidationException("El ID de producto es obligatorio.");
                }
                if (item.Cantidad <= 0)
                {
                    throw new OrdersValidationException("La cantidad debe ser mayor a cero.");
                }

                // Verificar si el producto existe (ORD-004)
                var product = _productsDb.FirstOrDefault(p => p.Id == item.ProductoId);
                if (product is null)
                {
                    throw new ProductNotFoundException(item.ProductoId);
                }

                // Verificar stock suficiente (ORD-005)
                if (product.Stock < item.Cantidad)
                {
                    throw new InsufficientStockException(
                        $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {item.Cantidad}.");
                }

                // Descontar stock (simulación de persistencia)
                product.Stock -= item.Cantidad;

                orderItems.Add(new OrderItem
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = product.Precio
                });
            }

            // Crear la orden de compra
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Items = orderItems.ToArray(),
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            _orders.Add(order);

            return new OrderResponse
            {
                Id = order.Id,
                UsuarioId = order.UsuarioId,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario
                }).ToList(),
                Total = order.Total,
                Estado = order.Estado,
                FechaCreacion = order.FechaCreacion
            };
        }
    }

    public UpdateOrderStatusResponse UpdateOrderStatus(Guid id, UpdateOrderStatusRequest request)
    {
        lock (_syncRoot)
        {
            // Simular un error interno si se pasa el UUID mágico de prueba para 500 Internal Server Error
            if (id == Guid.Parse("99999999-9999-9999-9999-999999999999"))
            {
                throw new InvalidOperationException("Error inesperado en la persistencia de datos al actualizar el estado de la orden (Simulado).");
            }

            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
            {
                throw new OrdersNotFoundException("ORD-001", "Orden no encontrada.");
            }

            if (!IsTransitionValid(order.Estado, request.Estado))
            {
                throw new OrdersConflictException($"Una orden en estado '{order.Estado}' no puede cambiar a '{request.Estado}'.");
            }

            order.Estado = request.Estado;
            var now = DateTime.UtcNow;
            order.FechaActualizacion = now;

            return new UpdateOrderStatusResponse
            {
                Id = order.Id,
                Estado = order.Estado,
                FechaActualizacion = now
            };
        }
    }

    private static bool IsTransitionValid(string current, string next)
    {
        if (current == next) return true;

        return current switch
        {
            "Pendiente" => next is "Confirmada" or "Cancelada",
            "Confirmada" => next is "Enviada" or "Cancelada",
            "Enviada" => next is "Entregada",
            "Entregada" => false,
            "Cancelada" => false,
            _ => false
        };
    }
}

