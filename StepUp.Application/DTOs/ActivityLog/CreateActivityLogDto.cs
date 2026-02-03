using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.ActivityLog;

public class CreateActivityLogDto
{
    public Guid UserId { get; set; }
    public decimal MetricValue { get; set; }
    public DateTime Date { get; set; }
    public MetricType MetricType { get; set; } = MetricType.Steps; // Default to Steps
    public string? ExerciseType { get; set; } // Only for PhysicalExercises
    public Guid? ParentActivityLogId { get; set; } // Link to source activity (auto-generated logs)
}

