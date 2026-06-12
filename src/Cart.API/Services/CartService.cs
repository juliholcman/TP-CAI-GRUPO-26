using System.Net;
using System.Net.Http.Json;
using Cart.API.Data.Repositories;
using Cart.API.DTOs;
using Cart.API.DTOs.Requests;
using Cart.API.DTOs.Responses;
using Cart.API.Exceptions;
using Cart.API.Models;

namespace Cart.API.Services;

public class CartService
{
    private readonly CartRepository _cartRepository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CartService> _logger;

    public CartService(
        CartRepository cartRepository,
        HttpClient httpClient,
        ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<CartResponse> GetCartAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart is null)
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        return MapToResponse(cart);
    }

    // Keep synchronous overload to avoid breaking CartController (non-async action)
    public CartResponse GetCart(Guid userId)
        => GetCartAsync(userId).GetAwaiter().GetResult();

    public async Task<CartResponse> AddItemAsync(Guid userId, AddCartItemRequest request)
    {
        if (request.Cantidad <= 0)
        {
            throw new ValidationException("CRT-004", "La cantidad debe ser mayor que cero.");
        }

        // Validate product exists in Products API and check stock
        var product = await FetchProductAsync(request.ProductId);

        // Ensure cart row exists (upsert)
        await _cartRepository.CreateOrUpdateCartAsync(userId);

        // Calculate total quantity (existing + requested)
        int existingQty = 0;
        if (await _cartRepository.ItemExistsAsync(userId, request.ProductId))
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            existingQty = cart?.Items.FirstOrDefault(i => i.ProductId == request.ProductId)?.Cantidad ?? 0;
        }

        int totalNewQuantity = existingQty + request.Cantidad;

        _logger.LogInformation(
            "Adding item to cart for user {UserId}. Existing: {HasExisting} (Qty: {ExistingQty}), Request Qty: {ReqQty}, Total: {TotalNewQty}, Stock: {Stock}",
            userId, existingQty > 0, existingQty, request.Cantidad, totalNewQuantity, product.Stock);

        if (totalNewQuantity > product.Stock)
        {
            throw new BusinessRuleException("CRT-003", "No hay stock suficiente para agregar la cantidad solicitada al carrito.");
        }

        if (existingQty > 0)
        {
            // Increment existing item quantity
            await _cartRepository.UpdateItemQuantityAsync(userId, request.ProductId, totalNewQuantity);
        }
        else
        {
            // Insert new item
            var newItem = new CartItem
            {
                ProductId = product.Id,
                Nombre    = product.Nombre,
                Precio    = product.Precio,
                Cantidad  = request.Cantidad
            };
            await _cartRepository.AddOrUpdateItemAsync(userId, newItem);
        }

        var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
        return MapToResponse(updatedCart!);
    }

    public async Task<CartResponse> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequest request)
    {
        if (request.Cantidad <= 0)
        {
            throw new ValidationException("CRT-004", "La cantidad debe ser mayor que cero.");
        }

        if (!await _cartRepository.CartExistsAsync(userId))
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        if (!await _cartRepository.ItemExistsAsync(userId, productId))
        {
            throw new NotFoundException("CRT-002", "Producto no encontrado.");
        }

        // Validate stock in Products API
        var product = await FetchProductAsync(productId);

        if (request.Cantidad > product.Stock)
        {
            throw new BusinessRuleException("CRT-003", "No hay stock suficiente para actualizar el producto a la cantidad solicitada.");
        }

        await _cartRepository.UpdateItemPriceAndQuantityAsync(userId, productId, request.Cantidad, product.Precio);
        await _cartRepository.CreateOrUpdateCartAsync(userId); // update timestamp

        var updatedCart = await _cartRepository.GetByUserIdAsync(userId);
        return MapToResponse(updatedCart!);
    }

    public async Task RemoveItemAsync(Guid userId, Guid productId)
    {
        if (!await _cartRepository.CartExistsAsync(userId))
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        if (!await _cartRepository.ItemExistsAsync(userId, productId))
        {
            throw new NotFoundException("CRT-002", "Producto no encontrado.");
        }

        await _cartRepository.RemoveItemAsync(userId, productId);
    }

    // Keep synchronous overload to avoid breaking CartController (non-async action)
    public void RemoveItem(Guid userId, Guid productId)
        => RemoveItemAsync(userId, productId).GetAwaiter().GetResult();

    public async Task ClearCartAsync(Guid userId)
    {
        if (!await _cartRepository.CartExistsAsync(userId))
        {
            throw new NotFoundException("CRT-001", "Carrito no encontrado.");
        }

        await _cartRepository.ClearCartAsync(userId);
    }

    // Keep synchronous overload to avoid breaking CartController (non-async action)
    public void ClearCart(Guid userId)
        => ClearCartAsync(userId).GetAwaiter().GetResult();

    // ── Private helpers ───────────────────────────────────────────────────────

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
            if (product is null)
            {
                throw new NotFoundException("CRT-002", "Producto no encontrado.");
            }

            _logger.LogInformation(
                "Fetched product {ProductId}: Name={Name}, Stock={Stock}, Price={Price}",
                productId, product.Nombre, product.Stock, product.Precio);

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

    private static CartResponse MapToResponse(Cart.API.Models.Cart cart)
    {
        return new CartResponse
        {
            UserId = cart.UserId,
            Total  = cart.Total,
            Items  = cart.Items.Select(item => new CartItemResponse
            {
                ProductId = item.ProductId,
                Nombre    = item.Nombre,
                Precio    = item.Precio,
                Cantidad  = item.Cantidad,
                Subtotal  = item.Subtotal
            }).ToList()
        };
    }
}
