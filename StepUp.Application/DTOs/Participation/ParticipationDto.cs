namespace StepUp.Application.DTOs.Participation;

public class ParticipationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ChallengeId { get; set; }
    public decimal TotalScore { get; set; }
}

