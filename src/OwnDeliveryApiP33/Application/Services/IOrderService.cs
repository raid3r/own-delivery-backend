using OwnDeliveryApiP33.Application.DTOs;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Application.Services;

public interface IOrderService : IApplicationService
{
    /// <summary>Create new order</summary>
    Task<OrderResponse> CreateOrderAsync(Guid customerId, CreateOrderRequest request, CancellationToken ct = default);

    /// <summary>Get order by ID</summary>
    Task<OrderResponse> GetOrderAsync(Guid orderId, CancellationToken ct = default);

    /// <summary>Get order by order number</summary>
    Task<OrderResponse> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);

    /// <summary>Get customer's orders</summary>
    Task<IEnumerable<OrderResponse>> GetCustomerOrdersAsync(Guid customerId, int skip = 0, int take = 20, CancellationToken ct = default);

    /// <summary>Get courier's orders</summary>
    Task<IEnumerable<OrderResponse>> GetCourierOrdersAsync(Guid courierId, int skip = 0, int take = 20, CancellationToken ct = default);

    /// <summary>Update order status</summary>
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, OrderStatusUpdateRequest request, CancellationToken ct = default);

    /// <summary>Cancel order</summary>
    Task<bool> CancelOrderAsync(Guid orderId, string reason, CancellationToken ct = default);

    /// <summary>Rate order and courier</summary>
    Task<bool> RateOrderAsync(Guid orderId, RateOrderRequest request, CancellationToken ct = default);
}
