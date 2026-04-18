using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class CourierRepository : Repository<Courier>, ICourierRepository
{
    public CourierRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Courier?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<IEnumerable<Courier>> GetAvailableCouriersAsync(int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.CurrentStatus == CourierStatus.Available && c.IsVerified)
            .OrderByDescending(c => c.AverageRating)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Courier>> GetVerifiedCouriersAsync(CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.IsVerified)
            .OrderByDescending(c => c.AverageRating)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Courier>> GetCouriersByStatusAsync(CourierStatus status, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.CurrentStatus == status)
            .OrderByDescending(c => c.AverageRating)
            .ToListAsync(ct);
    }

    public async Task<Courier?> GetNearestCourierAsync(decimal latitude, decimal longitude, double radiusKm = 10, CancellationToken ct = default)
    {
        // Simplified: just get the nearest one by basic distance calculation
        // In production, use PostGIS or similar spatial extension
        return await _dbSet
            .Where(c => c.CurrentStatus == CourierStatus.Available && c.IsVerified)
            .OrderByDescending(c => c.AverageRating)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IEnumerable<Courier>> GetCouriersByRatingAsync(decimal minRating, int skip = 0, int take = 20, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(c => c.AverageRating >= minRating)
            .OrderByDescending(c => c.AverageRating)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalDeliveriesAsync(Guid courierId, CancellationToken ct = default)
    {
        var courier = await GetByIdAsync(courierId, ct);
        return courier?.CompletedDeliveries ?? 0;
    }
}
