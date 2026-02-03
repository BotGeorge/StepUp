using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.Challenge;

public class CreateSponsoredChallengeDto
{
    public string Name { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public string ChallengeMode { get; set; } = string.Empty; // "endless" sau "target"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Description { get; set; }
    public string Prize { get; set; } = string.Empty; // Premiul pentru challenge-ul sponsorizat
    public decimal? TargetValue { get; set; } // Nullable - setat doar dacă ChallengeMode = "target"
    public string? ExerciseType { get; set; } // Nullable - setat doar pentru PhysicalExercises
    public bool IsPublic { get; set; } = true; // Dacă challenge-ul este public (true) sau private (false - doar prietenii pot accesa)
}

