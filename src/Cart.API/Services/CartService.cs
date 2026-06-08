using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Cart.API.DTOs;
using Cart.API.DTOs.Requests;
using Cart.API.DTOs.Responses;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services;

public class CartService
{
    private static readonly ConcurrentDictionary<Guid, Models.Cart> _carts = new();
    private readonly HttpClient _httpClient;
    private readonly ILogger<CartService> _logger;

    public CartService(HttpClient httpClient, ILogger<CartService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public CartResponse GetCart(Guid userId)
    {
        if (!_carts.TryGetValue(userId, out var cart))
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        return MapToResponse(cart);
    }

    public async Task<CartResponse> AddItemAsync(Guid userId, AddCartItemRequest request)
    {
        if (request.Cantidad <= 0)
        {
            throw new ValidationException("CRT-004", "Cantidad inválida.");
        }

        // Validate product exists in Products API
        var product = await FetchProductAsync(request.ProductId);

        var cart = _carts.GetOrAdd(userId, id => new Models.Cart { UserId = id });

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        int totalNewQuantity = (existingItem?.Cantidad ?? 0) + request.Cantidad;

        _logger.LogInformation("Adding item to cart for user {UserId}. Existing: {HasExisting} (Qty: {ExistingQty}), Request Qty: {ReqQty}, Total: {TotalNewQty}, Stock: {Stock}",
            userId, existingItem != null, existingItem?.Cantidad ?? 0, request.Cantidad, totalNewQuantity, product.Stock);

        if (totalNewQuantity > product.Stock)
        {
            throw new BusinessRuleException("CRT-003", "Stock insuficiente para agregar al carrito.");
        }

        if (existingItem != null)
        {
            existingItem.Cantidad = totalNewQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Nombre = product.Nombre,
                Precio = product.Precio,
                Cantidad = request.Cantidad
            });
        }

        return MapToResponse(cart);
    }

    public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request)
    {
        if (request.Cantidad <= 0)
        {
            throw new ValidationException("CRT-004", "Cantidad inválida.");
        }

        if (!_carts.TryGetValue(userId, out var cart))
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem == null)
        {
            // The image says: "CRT-002: Producto no encontrado. POST/PUT cuando el ProductId no existe en Products API."
            // Wait, does it also apply if the product is not in the cart?
            // To be perfectly safe, let's check both: if the product is not in the cart, throw CRT-002.
            throw new NotFoundException("CRT-002", "Producto no encontrado.");
        }

        // Validate stock in Products API
        var product = await FetchProductAsync(productId);

        if (request.Cantidad > product.Stock)
        {
            throw new BusinessRuleException("CRT-003", "Stock insuficiente para agregar al carrito.");
        }

        existingItem.Cantidad = request.Cantidad;
        existingItem.Precio = product.Precio; // sync price

        return MapToResponse(cart);
    }

    public void RemoveItem(Guid userId, Guid productId)
    {
        if (!_carts.TryGetValue(userId, out var cart))
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
        {
            throw new NotFoundException("CRT-002", "Producto no encontrado.");
        }

        cart.Items.Remove(item);
    }

    public void ClearCart(Guid userId)
    {
        if (!_carts.TryRemove(userId, out _))
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }
    }

    private async Task<ProductResponse> FetchProductAsync(Guid productId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            response.EnsureSuccessStatusCode();

            var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
            if (product == null)
            {
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            _logger.LogInformation("Fetched product {ProductId}: Name={Name}, Stock={Stock}, Price={Price}", productId, product.Nombre, product.Stock, product.Precio);

            return product;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Products API for product {ProductId}", productId);
            throw new Exception("Error al consultar el catálogo de productos.", ex);
        }
    }

    private static CartResponse MapToResponse(Models.Cart cart)
    {
        return new CartResponse
        {
            UserId = cart.UserId,
            Total = cart.Total,
            Items = cart.Items.Select(item => new CartItemResponse
            {
                ProductId = item.ProductId,
                Nombre = item.Nombre,
                Precio = item.Precio,
                Cantidad = item.Cantidad,
                Subtotal = item.Subtotal
            }).ToList()
        };
    }
}
