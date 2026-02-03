using StepUp.Application.DTOs.Challenge;
using StepUp.Application.DTOs.Leaderboard;

namespace StepUp.Application.Interfaces;

public interface IChallengeService
{
    Task<ChallengeDto> CreateChallengeAsync(CreateChallengeDto createChallengeDto, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<ChallengeDto> CreateSponsoredChallengeAsync(CreateSponsoredChallengeDto createSponsoredChallengeDto, Guid partnerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChallengeDto>> GetAllChallengesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChallengeDto>> GetActiveChallengesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChallengeDto>> GetVisibleChallengesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChallengeWithStatsDto>> GetAllChallengesWithStatsAsync(CancellationToken cancellationToken = default);
    Task<ChallengeDto?> GetChallengeByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default);
    Task CalculateAndUpdateScoresAsync(Guid challengeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(Guid challengeId, CancellationToken cancellationToken = default);
    Task MarkExpiredChallengesAsCompletedAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteChallengeAsync(Guid challengeId, Guid userId, CancellationToken cancellationToken = default);
}
