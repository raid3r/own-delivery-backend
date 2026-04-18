using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface ICourierRepository : IRepository<Courier>
{
    Task<Courier?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Courier>> GetAvailableCouriersAsync(int skip = 0, int take = 20, CancellationToken ct = default);
    Task<IEnumerable<Courier>> GetVerifiedCouriersAsync(CancellationToken ct = default);
    Task<IEnumerable<Courier>> GetCouriersByStatusAsync(CourierStatus status, CancellationToken ct = default);
    Task<Courier?> GetNearestCourierAsync(decimal latitude, decimal longitude, double radiusKm = 10, CancellationToken ct = default);
    Task<IEnumerable<Courier>> GetCouriersByRatingAsync(decimal minRating, int skip = 0, int take = 20, CancellationToken ct = default);
    Task<int> GetTotalDeliveriesAsync(Guid courierId, CancellationToken ct = default);
}
