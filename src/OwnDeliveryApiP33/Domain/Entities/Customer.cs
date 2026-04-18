namespace OwnDeliveryApiP33.Domain.Entities;

public class Customer : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string? PreferredDeliveryAddress { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalOrders { get; set; }
    public int SuccessfulOrders { get; set; }
    public int CancelledOrders { get; set; }
    
    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
