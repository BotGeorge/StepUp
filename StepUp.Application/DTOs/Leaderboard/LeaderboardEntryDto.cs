namespace StepUp.Application.DTOs.Leaderboard;

public class LeaderboardEntryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
}

