using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Domain.Entities;

public class Rating : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public Guid CourierId { get; set; }
    public Courier Courier { get; set; } = null!;
    
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public int Score { get; set; } // 1-5
    public string? Comment { get; set; }
    public RatingType Type { get; set; } = RatingType.Delivery;
}
