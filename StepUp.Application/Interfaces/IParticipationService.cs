using StepUp.Application.DTOs.Participation;

namespace StepUp.Application.Interfaces;

public interface IParticipationService
{
    Task<ParticipationDto> JoinChallengeAsync(JoinChallengeDto joinChallengeDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ParticipationDto>> GetUserParticipationsAsync(Guid userId, CancellationToken cancellationToken = default);
}

