using StepUp.Domain.Enums;

namespace StepUp.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.User;
    public bool IsSuspended { get; set; } = false;
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? LastActiveAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public int? Age { get; set; }
    public Gender Gender { get; set; } = Gender.Unspecified;
    public int DailyStepsGoal { get; set; } = 10000;

    // Navigation properties
    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}

