namespace StepUp.Application.DTOs.ActivityLog;

public class ActivitySummaryDto
{
    public Guid UserId { get; set; }
    public int CurrentStreakDays { get; set; }
    public int BestStreakDays { get; set; }
    public int TotalActivities { get; set; }
    public int TotalSteps { get; set; }
    public int TotalCalories { get; set; }
    public List<string> Achievements { get; set; } = new();
}
