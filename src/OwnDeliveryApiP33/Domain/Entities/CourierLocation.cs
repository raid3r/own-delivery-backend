using OwnDeliveryApiP33.Domain.ValueObjects;

namespace OwnDeliveryApiP33.Domain.Entities;

public class CourierLocation : BaseEntity
{
    public Guid CourierId { get; set; }
    public Courier Courier { get; set; } = null!;
    public Location Location { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
