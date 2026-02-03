using StepUp.Domain.Enums;

namespace StepUp.Domain.Entities;

public class Challenge : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public MetricType MetricType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Draft;
    public decimal? TargetValue { get; set; } // Nullable - dacă e null, challenge-ul este "endless"
    public string? ExerciseType { get; set; } // Nullable - folosit doar pentru PhysicalExercises (ex: "Flotări", "Abdomene", etc.)
    public string? Description { get; set; } // Nullable - descrierea challenge-ului
    public bool IsSponsored { get; set; } = false; // Dacă challenge-ul este sponsorizat
    public string? Prize { get; set; } // Nullable - premiul (setat doar dacă IsSponsored = true)
    public Guid? SponsorId { get; set; } // Nullable - ID-ul sponsorului (setat doar dacă IsSponsored = true)
    public Guid? CreatedByUserId { get; set; } // Nullable - creatorul challenge-ului
    public bool IsPublic { get; set; } = true; // Dacă challenge-ul este public (true) sau private (false - doar prietenii pot accesa)
    public DateTime? CompletedAt { get; set; } // Data finalizării (target atins sau timp expirat)
    public Guid? WinnerUserId { get; set; } // Câștigătorul challenge-ului

    // Navigation properties
    public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();
    public virtual User? Sponsor { get; set; } // Navigation property către User (sponsor)
    public virtual User? CreatedByUser { get; set; }
    public virtual User? WinnerUser { get; set; }
}

