using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetCustomerOrdersAsync(Guid customerId, int skip = 0, int take = 20, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetCourierOrdersAsync(Guid courierId, int skip = 0, int take = 20, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int skip = 0, int take = 20, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetUnassignedOrdersAsync(CancellationToken ct = default);
    Task<IEnumerable<Order>> GetOverdueOrdersAsync(CancellationToken ct = default);
    Task<int> GetCustomerOrderCountAsync(Guid customerId, CancellationToken ct = default);
    Task<decimal> GetAverageCostAsync(CancellationToken ct = default);
}
