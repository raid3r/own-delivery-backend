using OwnDeliveryApiP33.Domain.Enums;
using OwnDeliveryApiP33.Domain.ValueObjects;

namespace OwnDeliveryApiP33.Domain.Entities;

public class Courier : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string? LicenseNumber { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
    public CourierStatus CurrentStatus { get; set; } = CourierStatus.Offline;
    
    public decimal AverageRating { get; set; }
    public int TotalDeliveries { get; set; }
    public int CompletedDeliveries { get; set; }
    public int CancelledDeliveries { get; set; }
    public int AverageDeliveryTime { get; set; }
    
    public decimal TotalEarnings { get; set; }
    public string? BankAccount { get; set; }
    
    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<CourierLocation> Locations { get; set; } = new List<CourierLocation>();
    public ICollection<CourierDocument> Documents { get; set; } = new List<CourierDocument>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
