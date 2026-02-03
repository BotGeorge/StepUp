using StepUp.Application.DTOs.Challenge;

namespace StepUp.Application.DTOs.Friend;

public class FriendProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public List<ChallengeDto> Challenges { get; set; } = new List<ChallengeDto>();
    public FriendStatisticsDto Statistics { get; set; } = new FriendStatisticsDto();
}

public class FriendStatisticsDto
{
    public int TotalChallenges { get; set; }
    public int ActiveChallenges { get; set; }
    public int CompletedChallenges { get; set; }
    public int Victories { get; set; }
    public int TotalScore { get; set; }
}

