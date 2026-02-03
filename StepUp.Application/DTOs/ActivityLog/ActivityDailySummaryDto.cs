namespace StepUp.Application.DTOs.ActivityLog;

public class ActivityDailySummaryDto
{
    public DateTime Date { get; set; }
    public int Steps { get; set; }
    public int Calories { get; set; }
    public decimal Running { get; set; }
    public decimal PhysicalExercises { get; set; }
    public int TotalActivities { get; set; }
}
