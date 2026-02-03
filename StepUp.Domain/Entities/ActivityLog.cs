using StepUp.Domain.Enums;

namespace StepUp.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal MetricValue { get; set; }
    public MetricType MetricType { get; set; } = MetricType.Steps; // Default to Steps
    public string? ExerciseType { get; set; } // Nullable - used only for PhysicalExercises
    public Guid? ParentActivityLogId { get; set; } // Link to source activity (for auto-generated logs)

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ActivityLog? ParentActivityLog { get; set; }
    public virtual ICollection<ActivityLog> GeneratedLogs { get; set; } = new List<ActivityLog>();
}

