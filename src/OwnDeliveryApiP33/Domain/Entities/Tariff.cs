using OwnDeliveryApiP33.Domain.ValueObjects;

namespace OwnDeliveryApiP33.Domain.Entities;

public class Tariff : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public decimal BaseCost { get; set; }
    public decimal PricePerKm { get; set; }
    public decimal PricePerKg { get; set; }
    
    public int EstimatedDeliveryTime { get; set; } // в часах
    public decimal MaxWeight { get; set; }
    public Dimensions MaxDimensions { get; set; } = null!;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
