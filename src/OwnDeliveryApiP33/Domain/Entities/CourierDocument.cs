using OwnDeliveryApiP33.Domain.Enums;

namespace OwnDeliveryApiP33.Domain.Entities;

public class CourierDocument : BaseEntity
{
    public Guid CourierId { get; set; }
    public Courier Courier { get; set; } = null!;
    
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedBy { get; set; }
    public User? VerifiedByUser { get; set; }
    
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
