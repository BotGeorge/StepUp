using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.ActivityLog;

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal MetricValue { get; set; }
    public MetricType MetricType { get; set; } = MetricType.Steps;
    public string? ExerciseType { get; set; }
    public Guid? ParentActivityLogId { get; set; }
}

