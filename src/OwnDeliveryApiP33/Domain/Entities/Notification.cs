using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
