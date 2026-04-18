namespace OwnDeliveryApiP33.Domain.Entities;

public class Administrator : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public ICollection<string> Permissions { get; set; } = new List<string>();
    public ICollection<string> AssignedRegions { get; set; } = new List<string>();
    public ICollection<Guid> CreatedUsers { get; set; } = new List<Guid>();
}
