using OwnDeliveryApiP33.Domain.Entities;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, CancellationToken ct = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
    Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default);
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
}
