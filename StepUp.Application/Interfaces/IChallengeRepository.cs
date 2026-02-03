using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Interfaces;

public interface IChallengeRepository
{
    Task<Challenge> AddAsync(Challenge challenge, CancellationToken cancellationToken = default);
    Task<IEnumerable<Challenge>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Challenge>> GetActiveChallengesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Challenge>> GetVisibleChallengesForUserAsync(Guid userId, IEnumerable<Guid> friendIds, CancellationToken cancellationToken = default);
    Task<Challenge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Challenge>> GetExpiredChallengesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Challenge>> GetCompletedChallengesOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
    Task UpdateAsync(Challenge challenge, CancellationToken cancellationToken = default);
    Task DeleteAsync(Challenge challenge, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
