using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.Challenge;

public class CreateChallengeDto
{
    public string Name { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? TargetValue { get; set; } // Nullable - dacă e null, challenge-ul este "endless"
    public string? ExerciseType { get; set; } // Nullable - folosit doar pentru PhysicalExercises
    public string? Description { get; set; } // Nullable - descrierea challenge-ului
    public bool IsSponsored { get; set; } = false; // Dacă challenge-ul este sponsorizat
    public string? Prize { get; set; } // Nullable - premiul (setat doar dacă IsSponsored = true)
    public Guid? SponsorId { get; set; } // Nullable - ID-ul sponsorului (setat doar dacă IsSponsored = true)
    public bool IsPublic { get; set; } = true; // Dacă challenge-ul este public (true) sau private (false - doar prietenii pot accesa)
}

