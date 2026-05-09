using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.Services;

public interface IOrderService : IApplicationService
{
    /// <summary>Create order for a selected customer from the administration API</summary>
    Task<OrderResponse> AdminCreateOrderAsync(AdminCreateOrderRequest request, CancellationToken ct = default);

    /// <summary>Get paged orders for the administration API</summary>
    Task<PagedResponse<OrderResponse>> GetOrdersAsync(
        int skip = 0,
        int take = 20,
        OrderStatus? status = null,
        Guid? customerId = null,
        Guid? courierId = null,
        CancellationToken ct = default);

    /// <summary>Create new order</summary>
    Task<OrderResponse> CreateOrderAsync(Guid customerId, CreateOrderRequest request, CancellationToken ct = default);

    /// <summary>Get order by ID</summary>
    Task<OrderResponse> GetOrderAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>Get order by order number</summary>
    Task<OrderResponse> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);

    /// <summary>Get customer's orders</summary>
    Task<IEnumerable<OrderResponse>> GetCustomerOrdersAsync(Guid customerId, int skip = 0, int take = 20, CancellationToken ct = default);

    /// <summary>Get paged courier's orders</summary>
    Task<PagedResponse<OrderResponse>> GetCourierOrdersAsync(Guid courierId, int skip = 0, int take = 20, CancellationToken ct = default);

    /// <summary>Update order status</summary>
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, OrderStatusUpdateRequest request, CancellationToken ct = default);

    /// <summary>Cancel order</summary>
    Task<bool> CancelOrderAsync(Guid orderId, string reason, CancellationToken ct = default);

    /// <summary>Rate order and courier</summary>
    Task<bool> RateOrderAsync(Guid orderId, RateOrderRequest request, CancellationToken ct = default);

    /// <summary>Returns available (unassigned) orders a courier can accept.</summary>
    Task<PagedResponse<OrderResponse>> GetAvailableOrdersAsync(
        int skip = 0,
        int take = 20,
        decimal? lat = null,
        decimal? lon = null,
        decimal? radiusKm = null,
        CancellationToken ct = default);

    /// <summary>Assigns the courier linked to the authenticated user to an available order. Throws if already taken.</summary>
    Task<OrderResponse> AcceptOrderAsync(Guid orderId, Guid courierUserId, CancellationToken ct = default);
}
