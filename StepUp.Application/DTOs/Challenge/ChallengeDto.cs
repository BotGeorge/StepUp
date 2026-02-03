using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.Challenge;

public class ChallengeDto
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
    public string? Prize { get; set; } // Doar dacă există (IsSponsored = true)
    public string? SponsorName { get; set; } // Optional - numele sponsorului
    public Guid? CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? WinnerUserId { get; set; }
    public string? WinnerName { get; set; }
    public bool IsPublic { get; set; } = true; // Dacă challenge-ul este public sau private
}

