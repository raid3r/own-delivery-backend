using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class RatingRepository : Repository<Rating>, IRatingRepository
{
    public RatingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Rating>> GetCourierRatingsAsync(Guid courierId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(r => r.CourierId == courierId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Rating>> GetOrderRatingsAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(r => r.OrderId == orderId)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetAverageRatingAsync(Guid courierId, CancellationToken ct = default)
    {
        var ratings = await GetCourierRatingsAsync(courierId, ct);
        if (!ratings.Any())
            return 0;

        return (decimal)ratings.Average(r => r.Score);
    }

    public async Task<int> GetRatingCountAsync(Guid courierId, CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(r => r.CourierId == courierId, ct);
    }
}
