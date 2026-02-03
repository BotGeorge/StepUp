using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.Challenge;

public class ChallengeWithStatsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ChallengeStatus Status { get; set; }
    public decimal? TargetValue { get; set; }
    public string? ExerciseType { get; set; }
    public string? Description { get; set; }
    public bool IsSponsored { get; set; }
    public string? Prize { get; set; }
    public Guid? SponsorId { get; set; }
    public int ParticipantCount { get; set; }
    public bool IsUpcoming { get; set; }
    public bool IsActive { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? WinnerUserId { get; set; }
    public string? WinnerName { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
}

