using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "UAH";
    
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentMethod Method { get; set; }
    
    public string? TransactionId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? FailureReason { get; set; }
}
