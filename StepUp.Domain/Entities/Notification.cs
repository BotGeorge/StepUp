using StepUp.Domain.Enums;

namespace StepUp.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.ChallengeEnded;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
    public Guid? ChallengeId { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Challenge? Challenge { get; set; }
}
