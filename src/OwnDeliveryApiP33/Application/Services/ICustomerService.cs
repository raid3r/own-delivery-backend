using OwnDeliveryApiP33.Application.DTOs;

namespace OwnDeliveryApiP33.Application.Services;

public interface ICustomerService : IApplicationService
{
    /// <summary>Get customer profile</summary>
    Task<CustomerResponse> GetProfileAsync(Guid customerId, CancellationToken ct = default);

    /// <summary>Get customer by user ID</summary>
    Task<CustomerResponse> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Get top customers by order count</summary>
    Task<IEnumerable<CustomerResponse>> GetTopCustomersAsync(int count = 10, CancellationToken ct = default);

    /// <summary>Get customer statistics</summary>
    Task<CustomerStatsResponse> GetStatsAsync(Guid customerId, CancellationToken ct = default);
}

public record CustomerStatsResponse(
    int TotalOrders,
    int CompletedOrders,
    int CancelledOrders,
    int PendingOrders,
    decimal TotalSpent,
    decimal AverageOrderCost,
    decimal AverageRating);
