using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Domain.ValueObjects;

namespace OwnDeliveryApiP33.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    
    public Guid? CourierId { get; set; }
    public Courier? Courier { get; set; }
    
    public Guid TariffId { get; set; }
    public Tariff Tariff { get; set; } = null!;
    
    public OrderStatus Status { get; set; } = OrderStatus.New;
    
    // Addresses
    public Address PickupAddress { get; set; } = null!;
    public Address DeliveryAddress { get; set; } = null!;
    
    // Package info
    public decimal Weight { get; set; }
    public Dimensions Dimensions { get; set; } = null!;
    public string? Description { get; set; }
    public string? SpecialInstructions { get; set; }
    
    // Timing
    public DateTime? ScheduledDeliveryTime { get; set; }
    public DateTime? ActualPickupTime { get; set; }
    public DateTime? ActualDeliveryTime { get; set; }
    public DateTime? EstimatedDeliveryTime { get; set; }
    
    // Payment
    public decimal Cost { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public PaymentMethod? PaymentMethod { get; set; }
    
    // Additional info
    public string? Notes { get; set; }
    public string? CancelReason { get; set; }
    
    // Navigation
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public Payment? Payment { get; set; }
}
