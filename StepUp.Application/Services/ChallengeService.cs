using AutoMapper;
using StepUp.Application.DTOs.Challenge;
using StepUp.Application.DTOs.Leaderboard;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class ChallengeService : IChallengeService
{
    private readonly IChallengeRepository _challengeRepository;
    private readonly IParticipationRepository _participationRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IMapper _mapper;

    public ChallengeService(
        IChallengeRepository challengeRepository,
        IParticipationRepository participationRepository,
        IActivityLogRepository activityLogRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        IFriendRequestRepository friendRequestRepository,
        IMapper mapper)
    {
        _challengeRepository = challengeRepository;
        _participationRepository = participationRepository;
        _activityLogRepository = activityLogRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _friendRequestRepository = friendRequestRepository;
        _mapper = mapper;
    }

    public async Task<ChallengeDto> CreateChallengeAsync(CreateChallengeDto createChallengeDto, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var challenge = _mapper.Map<Challenge>(createChallengeDto);

        DateTime ToUtcDateOnly(DateTime dt)
        {
            var dateOnly = dt.Date;
            return dt.Kind == DateTimeKind.Utc
                ? dateOnly
                : DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
        }

        // Normalize dates to UTC date-only to avoid timezone drift
        challenge.StartDate = ToUtcDateOnly(challenge.StartDate);
        challenge.EndDate = ToUtcDateOnly(challenge.EndDate);
        
        // Validare: Challenge-urile sponsorizate pot fi create DOAR de utilizatori cu rolul Partner
        if (challenge.IsSponsored)
        {
            if (userId == null)
            {
                throw new InvalidOperationException("Trebuie să fii autentificat pentru a crea challenge-uri sponsorizate.");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user == null)
            {
                throw new InvalidOperationException("Utilizatorul nu a fost găsit.");
            }

            if (user.Role != Role.Partner)
            {
                throw new InvalidOperationException("Doar utilizatorii cu rolul Partner pot crea challenge-uri sponsorizate.");
            }

            // Setează SponsorId la userId-ul creatorului (care este Partner)
            challenge.SponsorId = userId.Value;

            // Dacă IsSponsored = true, verificăm că Prize este setat
            if (string.IsNullOrWhiteSpace(challenge.Prize))
            {
                throw new InvalidOperationException("Prize trebuie să fie setat când IsSponsored = true.");
            }
        }

        if (userId.HasValue)
        {
            challenge.CreatedByUserId = userId.Value;
        }

        if (challenge.MetricType == MetricType.CalorieBurn)
        {
            throw new InvalidOperationException("Challenge-urile de tip CalorieBurn nu sunt permise.");
        }

        if (challenge.MetricType == MetricType.PhysicalExercises && string.IsNullOrWhiteSpace(challenge.ExerciseType))
        {
            throw new InvalidOperationException("Trebuie să selectezi tipul de exerciții fizice pentru acest challenge.");
        }
        if (challenge.MetricType != MetricType.PhysicalExercises)
        {
            challenge.ExerciseType = null;
        }

        if (!challenge.IsSponsored)
        {
            // Dacă nu este sponsorizat, asigură-te că Prize și SponsorId sunt null
            challenge.Prize = null;
            challenge.SponsorId = null;
        }
        
        // Set status based on dates / target
        var now = DateTime.UtcNow;
        var hasTarget = challenge.TargetValue.HasValue && challenge.TargetValue.Value > 0;
        if (hasTarget)
        {
            challenge.Status = challenge.StartDate > now ? ChallengeStatus.Draft : ChallengeStatus.Active;
        }
        else if (challenge.StartDate <= now && challenge.EndDate >= now)
        {
            challenge.Status = ChallengeStatus.Active;
        }
        else if (challenge.StartDate > now)
        {
            challenge.Status = ChallengeStatus.Draft;
        }
        else
        {
            challenge.Status = ChallengeStatus.Completed;
        }

        challenge.CompletedAt = null;
        challenge.WinnerUserId = null;
        
        await _challengeRepository.AddAsync(challenge, cancellationToken);
        await _challengeRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ChallengeDto>(challenge);
    }

    public async Task<bool> DeleteChallengeAsync(Guid challengeId, Guid userId, CancellationToken cancellationToken = default)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId, cancellationToken);
        if (challenge == null)
        {
            return false;
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("Utilizatorul nu a fost găsit.");
        }

        var isOwner = challenge.CreatedByUserId.HasValue && challenge.CreatedByUserId.Value == userId;
        var isSponsor = challenge.SponsorId.HasValue && challenge.SponsorId.Value == userId;
        var isAdmin = user.Role == Role.Admin;

        if (!isOwner && !isSponsor && !isAdmin)
        {
            throw new InvalidOperationException("Nu ai dreptul să ștergi acest challenge.");
        }

        await _challengeRepository.DeleteAsync(challenge, cancellationToken);
        await _challengeRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ChallengeDto?> GetChallengeByIdAsync(Guid id, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var challenge = await _challengeRepository.GetByIdAsync(id, cancellationToken);
        if (challenge == null)
        {
            return null;
        }

        // Verifică dacă challenge-ul este privat și dacă utilizatorul poate accesa
        if (!challenge.IsPublic && userId.HasValue && challenge.CreatedByUserId.HasValue)
        {
            // Dacă utilizatorul este creatorul, poate accesa
            if (challenge.CreatedByUserId.Value == userId.Value)
            {
                return _mapper.Map<ChallengeDto>(challenge);
            }

            // Verifică dacă utilizatorul este prieten cu creatorul
            var areFriends = await _friendRequestRepository.AreFriendsAsync(
                userId.Value, 
                challenge.CreatedByUserId.Value, 
                cancellationToken);
            
            if (!areFriends)
            {
                // Challenge-ul este privat și utilizatorul nu este prieten cu creatorul
                throw new UnauthorizedAccessException("Acest challenge este privat. Trebuie să fii prieten cu creatorul pentru a-l accesa.");
            }
        }

        return _mapper.Map<ChallengeDto>(challenge);
    }

    public async Task<ChallengeDto> CreateSponsoredChallengeAsync(CreateSponsoredChallengeDto createSponsoredChallengeDto, Guid partnerId, CancellationToken cancellationToken = default)
    {
        // Convert CreateSponsoredChallengeDto to CreateChallengeDto
        // Challenge-urile sponsorizate trebuie să fie întotdeauna publice
        var createChallengeDto = new CreateChallengeDto
        {
            Name = createSponsoredChallengeDto.Name,
            MetricType = createSponsoredChallengeDto.MetricType,
            StartDate = createSponsoredChallengeDto.StartDate,
            EndDate = createSponsoredChallengeDto.EndDate,
            TargetValue = createSponsoredChallengeDto.TargetValue,
            ExerciseType = createSponsoredChallengeDto.ExerciseType,
            Description = createSponsoredChallengeDto.Description,
            IsSponsored = true,
            Prize = createSponsoredChallengeDto.Prize,
            SponsorId = partnerId,
            IsPublic = true // Challenge-urile sponsorizate sunt întotdeauna publice
        };

        return await CreateChallengeAsync(createChallengeDto, partnerId, cancellationToken);
    }

    public async Task<IEnumerable<ChallengeWithStatsDto>> GetActiveChallengesWithStatsAsync(CancellationToken cancellationToken = default)
    {
        await MarkExpiredChallengesAsCompletedAsync(cancellationToken);
        await CleanupCompletedChallengesAsync(cancellationToken);
        var challenges = await _challengeRepository.GetActiveChallengesAsync(cancellationToken);
        var now = DateTime.UtcNow;
        
        var challengesWithStats = new List<ChallengeWithStatsDto>();
        
        foreach (var challenge in challenges)
        {
            var participations = await _participationRepository.GetByChallengeIdAsync(challenge.Id, cancellationToken);
            var participantCount = participations.Count();
            
            var hasTarget = challenge.TargetValue.HasValue && challenge.TargetValue.Value > 0;
            var isUpcoming = challenge.StartDate > now;
            var isActive = challenge.Status == ChallengeStatus.Active 
                        && challenge.StartDate <= now 
                        && !challenge.CompletedAt.HasValue
                        && (hasTarget || challenge.EndDate >= now);
            var isCompleted = challenge.Status == ChallengeStatus.Completed
                           || challenge.CompletedAt.HasValue
                           || (!hasTarget && challenge.EndDate < now);
            
            challengesWithStats.Add(new ChallengeWithStatsDto
            {
                Id = challenge.Id,
                Name = challenge.Name,
                MetricType = challenge.MetricType,
                StartDate = challenge.StartDate,
                EndDate = challenge.EndDate,
                Status = challenge.Status,
                TargetValue = challenge.TargetValue,
                ExerciseType = challenge.ExerciseType,
                Description = challenge.Description,
                IsSponsored = challenge.IsSponsored,
                Prize = challenge.Prize,
                SponsorId = challenge.SponsorId,
                ParticipantCount = participantCount,
                IsUpcoming = isUpcoming,
                IsActive = isActive,
                IsCompleted = isCompleted,
                CompletedAt = challenge.CompletedAt,
                WinnerUserId = challenge.WinnerUserId,
                WinnerName = challenge.WinnerUser != null ? challenge.WinnerUser.Name : null,
                CreatedByUserId = challenge.CreatedByUserId,
                CreatedByName = challenge.CreatedByUser != null ? challenge.CreatedByUser.Name : null
            });
        }
        
        return challengesWithStats;
    }

    public async Task<IEnumerable<ChallengeDto>> GetAllChallengesAsync(CancellationToken cancellationToken = default)
    {
        var challenges = await _challengeRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ChallengeDto>>(challenges);
    }

    public async Task<IEnumerable<ChallengeDto>> GetActiveChallengesAsync(CancellationToken cancellationToken = default)
    {
        var challenges = await _challengeRepository.GetActiveChallengesAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ChallengeDto>>(challenges);
    }

    public async Task<IEnumerable<ChallengeDto>> GetVisibleChallengesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Obține lista de prieteni
        var friendRequests = await _friendRequestRepository.GetFriendsAsync(userId, cancellationToken);
        var friendIds = new List<Guid>();
        
        foreach (var request in friendRequests)
        {
            // Determină care user este prietenul (cel care nu este userId)
            var friendId = request.FromUserId == userId ? request.ToUserId : request.FromUserId;
            friendIds.Add(friendId);
        }

        // Obține challenge-urile vizibile pentru utilizator
        var challenges = await _challengeRepository.GetVisibleChallengesForUserAsync(userId, friendIds, cancellationToken);
        return _mapper.Map<IEnumerable<ChallengeDto>>(challenges);
    }

    public async Task<IEnumerable<ChallengeWithStatsDto>> GetAllChallengesWithStatsAsync(CancellationToken cancellationToken = default)
    {
        await MarkExpiredChallengesAsCompletedAsync(cancellationToken);
        await CleanupCompletedChallengesAsync(cancellationToken);
        var challenges = await _challengeRepository.GetAllAsync(cancellationToken);
        var now = DateTime.UtcNow;
        
        var challengesWithStats = new List<ChallengeWithStatsDto>();
        
        foreach (var challenge in challenges)
        {
            // Get participant count
            var participations = await _participationRepository.GetByChallengeIdAsync(challenge.Id, cancellationToken);
            var participantCount = participations.Count();
            
            // Determine status flags
            var hasTarget = challenge.TargetValue.HasValue && challenge.TargetValue.Value > 0;
            var isUpcoming = challenge.StartDate > now;
            var isActive = challenge.Status == ChallengeStatus.Active 
                        && challenge.StartDate <= now 
                        && !challenge.CompletedAt.HasValue
                        && (hasTarget || challenge.EndDate >= now);
            var isCompleted = challenge.Status == ChallengeStatus.Completed
                           || challenge.CompletedAt.HasValue
                           || (!hasTarget && challenge.EndDate < now);
            
            challengesWithStats.Add(new ChallengeWithStatsDto
            {
                Id = challenge.Id,
                Name = challenge.Name,
                MetricType = challenge.MetricType,
                StartDate = challenge.StartDate,
                EndDate = challenge.EndDate,
                Status = challenge.Status,
                TargetValue = challenge.TargetValue,
                ExerciseType = challenge.ExerciseType,
                Description = challenge.Description,
                IsSponsored = challenge.IsSponsored,
                Prize = challenge.Prize,
                SponsorId = challenge.SponsorId,
                ParticipantCount = participantCount,
                IsUpcoming = isUpcoming,
                IsActive = isActive,
                IsCompleted = isCompleted,
                CompletedAt = challenge.CompletedAt,
                WinnerUserId = challenge.WinnerUserId,
                WinnerName = challenge.WinnerUser != null ? challenge.WinnerUser.Name : null,
                CreatedByUserId = challenge.CreatedByUserId,
                CreatedByName = challenge.CreatedByUser != null ? challenge.CreatedByUser.Name : null
            });
        }
        
        return challengesWithStats;
    }

    public async Task CalculateAndUpdateScoresAsync(Guid challengeId, CancellationToken cancellationToken = default)
    {
        // 1. Verifică că challenge-ul există
        var challenge = await _challengeRepository.GetByIdAsync(challengeId, cancellationToken);
        if (challenge == null)
        {
            throw new InvalidOperationException($"Challenge with ID {challengeId} does not exist.");
        }

        if (challenge.Status == ChallengeStatus.Completed && challenge.CompletedAt.HasValue)
        {
            return;
        }

        // 2. Obține toate participările pentru acest challenge
        var participations = await _participationRepository.GetByChallengeIdAsync(challengeId, cancellationToken);

        DateTime ToUtcDateOnly(DateTime dt)
        {
            var dateOnly = dt.Date;
            return dt.Kind == DateTimeKind.Utc
                ? dateOnly
                : DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
        }

        // Normalize range to date-only to align with ActivityLog.Date (stored as UTC date)
        var rangeStart = ToUtcDateOnly(challenge.StartDate);
        var targetValue = challenge.TargetValue;
        var hasTarget = targetValue.HasValue && targetValue.Value > 0;
        var effectiveEnd = challenge.CompletedAt ?? (hasTarget ? DateTime.UtcNow : challenge.EndDate);
        var rangeEnd = ToUtcDateOnly(effectiveEnd);

            // 3. Pentru fiecare participare, calculează TotalScore
            foreach (var participation in participations)
            {
                // Only count activities logged after the user joined the challenge.
                var joinedAt = participation.CreatedAt;
                var joinedDate = ToUtcDateOnly(joinedAt);
                var effectiveStart = joinedDate > rangeStart ? joinedDate : rangeStart;

                var totalScore = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
                    participation.UserId,
                    effectiveStart,
                    rangeEnd,
                    challenge.MetricType,
                    challenge.MetricType == MetricType.PhysicalExercises ? challenge.ExerciseType : null,
                    joinedAt,
                    cancellationToken);

                // Actualizează TotalScore în Participation
                participation.TotalScore = totalScore;
                await _participationRepository.UpdateAsync(participation, cancellationToken);
            }

        await _participationRepository.SaveChangesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var isExpired = challenge.EndDate < now;

        if (challenge.Status == ChallengeStatus.Active)
        {
            if (hasTarget && participations.Any(p => p.TotalScore >= targetValue!.Value))
            {
                await CompleteChallengeAsync(challenge, participations, isTargetReached: true, cancellationToken);
            }
            else if (!hasTarget && isExpired)
            {
                await CompleteChallengeAsync(challenge, participations, isTargetReached: false, cancellationToken);
            }
        }
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(Guid challengeId, CancellationToken cancellationToken = default)
    {
        // 1. Verifică că challenge-ul există
        var challenge = await _challengeRepository.GetByIdAsync(challengeId, cancellationToken);
        if (challenge == null)
        {
            throw new InvalidOperationException($"Challenge with ID {challengeId} does not exist.");
        }

        // 2. Obține toate participările pentru acest challenge cu User inclus, sortate descrescător după TotalScore
        var participations = await _participationRepository.GetByChallengeIdWithUserAsync(challengeId, cancellationToken);

        // 3. Mapează la LeaderboardEntryDto
        var leaderboard = participations.Select(p => new LeaderboardEntryDto
        {
            UserId = p.UserId,
            UserName = p.User.Name,
            TotalScore = p.TotalScore
        });

        return leaderboard;
    }

    public async Task MarkExpiredChallengesAsCompletedAsync(CancellationToken cancellationToken = default)
    {
        var expiredChallenges = await _challengeRepository.GetExpiredChallengesAsync(cancellationToken);
        
        foreach (var challenge in expiredChallenges)
        {
            await CalculateAndUpdateScoresAsync(challenge.Id, cancellationToken);
        }
        
        await _challengeRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task CleanupCompletedChallengesAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-1);
        var expiredCompleted = await _challengeRepository.GetCompletedChallengesOlderThanAsync(cutoff, cancellationToken);
        foreach (var challenge in expiredCompleted)
        {
            await _challengeRepository.DeleteAsync(challenge, cancellationToken);
        }

        if (expiredCompleted.Any())
        {
            await _challengeRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CompleteChallengeAsync(
        Challenge challenge,
        IEnumerable<Participation> participations,
        bool isTargetReached,
        CancellationToken cancellationToken)
    {
        var participationsWithUser = await _participationRepository.GetByChallengeIdWithUserAsync(challenge.Id, cancellationToken);
        var winner = participationsWithUser
            .OrderByDescending(p => p.TotalScore)
            .ThenBy(p => p.CreatedAt)
            .FirstOrDefault();

        if (winner != null)
        {
            challenge.WinnerUserId = winner.UserId;
        }

        challenge.Status = ChallengeStatus.Completed;
        challenge.CompletedAt = DateTime.UtcNow;

        await _challengeRepository.UpdateAsync(challenge, cancellationToken);
        await _challengeRepository.SaveChangesAsync(cancellationToken);

        if (winner != null)
        {
            var participantUserIds = participations.Select(p => p.UserId).ToList();
            await _notificationService.CreateChallengeCompletionNotificationsAsync(
                challenge.Id,
                challenge.Name,
                winner.UserId,
                participantUserIds,
                cancellationToken);
        }
    }
}

