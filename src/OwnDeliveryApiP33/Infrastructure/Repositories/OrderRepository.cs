using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    private const double EarthRadiusKm = 6371d;

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

    public async Task<IEnumerable<Order>> GetUnassignedOrdersAsync(
        int skip = 0,
        int take = 20,
        decimal? lat = null,
        decimal? lon = null,
        decimal? radiusKm = null,
        CancellationToken ct = default)
    {
        var query = BuildUnassignedOrdersQuery(lat, lon, radiusKm);

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountUnassignedOrdersAsync(
        decimal? lat = null,
        decimal? lon = null,
        decimal? radiusKm = null,
        CancellationToken ct = default)
    {
        return await BuildUnassignedOrdersQuery(lat, lon, radiusKm).CountAsync(ct);
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

    private IQueryable<Order> BuildUnassignedOrdersQuery(decimal? lat, decimal? lon, decimal? radiusKm)
    {
        var query = _dbSet.Where(o => o.CourierId == null && (o.Status == OrderStatus.Pending || o.Status == OrderStatus.PickedUp));

        if (!lat.HasValue || !lon.HasValue || !radiusKm.HasValue)
            return query;

        var latitude = (double)lat.Value;
        var longitude = (double)lon.Value;
        var radius = (double)radiusKm.Value;
        var latRad = latitude * Math.PI / 180d;
        var lonRad = longitude * Math.PI / 180d;

        return query.Where(o =>
            EarthRadiusKm * 2d * Math.Asin(
                Math.Sqrt(
                    Math.Pow(Math.Sin((((double)o.PickupAddress.Latitude * Math.PI / 180d) - latRad) / 2d), 2d) +
                    Math.Cos(latRad) * Math.Cos((double)o.PickupAddress.Latitude * Math.PI / 180d) *
                    Math.Pow(Math.Sin((((double)o.PickupAddress.Longitude * Math.PI / 180d) - lonRad) / 2d), 2d)
                )) <= radius);
    }
}
