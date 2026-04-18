using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface IRatingRepository : IRepository<Rating>
{
    Task<IEnumerable<Rating>> GetCourierRatingsAsync(Guid courierId, CancellationToken ct = default);
    Task<IEnumerable<Rating>> GetOrderRatingsAsync(Guid orderId, CancellationToken ct = default);
    Task<decimal> GetAverageRatingAsync(Guid courierId, CancellationToken ct = default);
    Task<int> GetRatingCountAsync(Guid courierId, CancellationToken ct = default);
}
