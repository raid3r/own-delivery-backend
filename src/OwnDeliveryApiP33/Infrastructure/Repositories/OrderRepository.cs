using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);
    }

    public async Task<IEnumerable<Order>> GetCustomerOrdersAsync(Guid customerId, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Order>> GetCourierOrdersAsync(Guid courierId, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(o => o.CourierId == courierId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Order>> GetUnassignedOrdersAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(o => o.CourierId == null && (o.Status == OrderStatus.New || o.Status == OrderStatus.WaitingForCourier))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Order>> GetOverdueOrdersAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(o => o.EstimatedDeliveryTime < DateTime.UtcNow && o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetCustomerOrderCountAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(o => o.CustomerId == customerId, ct);
    }

    public async Task<decimal> GetAverageCostAsync(CancellationToken ct = default)
    {
        var totalCount = await _dbSet.CountAsync();
        if (totalCount == 0)
            return 0;

        var totalCost = await _dbSet.SumAsync(o => o.Cost);
        return totalCost / totalCount;
    }
}
