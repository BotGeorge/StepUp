using StepUp.Domain.Enums;

namespace StepUp.Domain.Entities;

public class FriendRequest : BaseEntity
{
    public Guid FromUserId { get; set; }
    public Guid ToUserId { get; set; }
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;

    // Navigation properties
    public virtual User FromUser { get; set; } = null!;
    public virtual User ToUser { get; set; } = null!;
}

