using AutoMapper;
using StepUp.Application.DTOs.Participation;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class ParticipationService : IParticipationService
{
    private readonly IParticipationRepository _participationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChallengeRepository _challengeRepository;
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IMapper _mapper;

    public ParticipationService(
        IParticipationRepository participationRepository,
        IUserRepository userRepository,
        IChallengeRepository challengeRepository,
        IFriendRequestRepository friendRequestRepository,
        IMapper mapper)
    {
        _participationRepository = participationRepository;
        _userRepository = userRepository;
        _challengeRepository = challengeRepository;
        _friendRequestRepository = friendRequestRepository;
        _mapper = mapper;
    }

    public async Task<ParticipationDto> JoinChallengeAsync(JoinChallengeDto joinChallengeDto, CancellationToken cancellationToken = default)
    {
        // 1. Verifică că userul există
        var user = await _userRepository.GetByIdAsync(joinChallengeDto.UserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {joinChallengeDto.UserId} does not exist.");
        }

        // 2. Verifică că challenge-ul există
        var challenge = await _challengeRepository.GetByIdAsync(joinChallengeDto.ChallengeId, cancellationToken);
        if (challenge == null)
        {
            throw new InvalidOperationException($"Challenge with ID {joinChallengeDto.ChallengeId} does not exist.");
        }

        // 3. Verifică că challenge-ul poate accepta participanți (activ sau viitor)
        var now = DateTime.UtcNow;
        var hasTarget = challenge.TargetValue.HasValue && challenge.TargetValue.Value > 0;
        var isActive = challenge.Status == ChallengeStatus.Active 
                    && challenge.StartDate <= now 
                    && !challenge.CompletedAt.HasValue
                    && (hasTarget ? true : challenge.EndDate >= now);
        var isUpcoming = challenge.StartDate > now && challenge.Status != ChallengeStatus.Cancelled;
        var isCompleted = challenge.Status == ChallengeStatus.Completed
                       || challenge.CompletedAt.HasValue
                       || (!hasTarget && challenge.EndDate < now);
        
        if (!isActive && !isUpcoming)
        {
            if (isCompleted)
            {
                throw new InvalidOperationException($"Challenge with ID {joinChallengeDto.ChallengeId} has already ended.");
            }
            if (challenge.Status == ChallengeStatus.Cancelled)
            {
                throw new InvalidOperationException($"Challenge with ID {joinChallengeDto.ChallengeId} has been cancelled.");
            }
            throw new InvalidOperationException($"Challenge with ID {joinChallengeDto.ChallengeId} is not available for joining.");
        }

        // 4. Verifică dacă challenge-ul este private și dacă utilizatorul poate accesa
        if (!challenge.IsPublic && challenge.CreatedByUserId.HasValue)
        {
            // Dacă challenge-ul este private, verifică dacă utilizatorul este creatorul sau prieten cu creatorul
            if (challenge.CreatedByUserId.Value != joinChallengeDto.UserId)
            {
                var areFriends = await _friendRequestRepository.AreFriendsAsync(
                    joinChallengeDto.UserId, 
                    challenge.CreatedByUserId.Value, 
                    cancellationToken);
                
                if (!areFriends)
                {
                    throw new InvalidOperationException("Acest challenge este privat. Trebuie să fii prieten cu creatorul pentru a te putea alătura.");
                }
            }
        }

        // 5. Verifică că userul nu este deja înscris
        var existingParticipation = await _participationRepository.ExistsAsync(
            joinChallengeDto.UserId, 
            joinChallengeDto.ChallengeId, 
            cancellationToken);
        
        if (existingParticipation)
        {
            throw new InvalidOperationException($"User {joinChallengeDto.UserId} is already registered for challenge {joinChallengeDto.ChallengeId}.");
        }

        // 5. Creează înregistrarea în Participation cu TotalScore = 0
        var participation = new Participation
        {
            UserId = joinChallengeDto.UserId,
            ChallengeId = joinChallengeDto.ChallengeId,
            TotalScore = 0
        };

        await _participationRepository.AddAsync(participation, cancellationToken);
        await _participationRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ParticipationDto>(participation);
    }

    public async Task<IEnumerable<ParticipationDto>> GetUserParticipationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var participations = await _participationRepository.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<ParticipationDto>>(participations);
    }
}

