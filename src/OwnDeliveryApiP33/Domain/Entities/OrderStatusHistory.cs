using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Domain.ValueObjects;

namespace OwnDeliveryApiP33.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public OrderStatus OldStatus { get; set; }
    public OrderStatus NewStatus { get; set; }
    
    public Guid ChangedBy { get; set; }
    public User ChangedByUser { get; set; } = null!;
    
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Location? Location { get; set; }
    public string? ProofUrl { get; set; }
}
