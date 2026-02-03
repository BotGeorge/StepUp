using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.ActivityLog;

public class UpdateActivityLogDto
{
    public Guid UserId { get; set; }
    public decimal MetricValue { get; set; }
    public DateTime Date { get; set; }
    public MetricType MetricType { get; set; } = MetricType.Steps;
    public string? ExerciseType { get; set; } // Only for PhysicalExercises
}
