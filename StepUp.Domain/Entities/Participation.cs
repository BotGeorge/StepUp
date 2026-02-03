namespace StepUp.Domain.Entities;

public class Participation : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ChallengeId { get; set; }
    public decimal TotalScore { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Challenge Challenge { get; set; } = null!;
}

