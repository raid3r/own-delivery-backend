using Microsoft.EntityFrameworkCore;
using OwnDeliveryApiP33.Domain.Entities;
using OwnDeliveryApiP33.Infrastructure.Data;

namespace OwnDeliveryApiP33.Infrastructure.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId, bool unreadOnly = false, CancellationToken ct = default)
    {
        var query = _dbSet.Where(n => n.UserId == userId);
        
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);
        
        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await GetByIdAsync(notificationId, ct);
        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await UpdateAsync(notification, ct);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (!notifications.Any())
            return false;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return true;
    }
}
