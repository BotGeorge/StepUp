using StepUp.Domain.Entities;

namespace StepUp.Application.Interfaces;

public interface IParticipationRepository
{
    Task<Participation> AddAsync(Participation participation, CancellationToken cancellationToken = default);
    Task<Participation?> GetByUserAndChallengeAsync(Guid userId, Guid challengeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Participation>> GetByChallengeIdAsync(Guid challengeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Participation>> GetByChallengeIdWithUserAsync(Guid challengeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Participation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid challengeId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Participation participation, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

