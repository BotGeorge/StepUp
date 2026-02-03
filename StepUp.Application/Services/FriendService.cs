using AutoMapper;
using StepUp.Application.DTOs.Challenge;
using StepUp.Application.DTOs.Friend;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class FriendService : IFriendService
{
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IParticipationRepository _participationRepository;
    private readonly IChallengeRepository _challengeRepository;
    private readonly IMapper _mapper;

    public FriendService(
        IFriendRequestRepository friendRequestRepository,
        IUserRepository userRepository,
        IParticipationRepository participationRepository,
        IChallengeRepository challengeRepository,
        IMapper mapper)
    {
        _friendRequestRepository = friendRequestRepository;
        _userRepository = userRepository;
        _participationRepository = participationRepository;
        _challengeRepository = challengeRepository;
        _mapper = mapper;
    }

    public async Task<FriendRequestDto> SendFriendRequestAsync(Guid fromUserId, Guid toUserId, CancellationToken cancellationToken = default)
    {
        // Validări
        if (fromUserId == toUserId)
        {
            throw new InvalidOperationException("Nu poți trimite cerere de prietenie către tine însuți.");
        }

        var fromUser = await _userRepository.GetByIdAsync(fromUserId, cancellationToken);
        if (fromUser == null)
        {
            throw new InvalidOperationException($"User with ID {fromUserId} does not exist.");
        }

        var toUser = await _userRepository.GetByIdAsync(toUserId, cancellationToken);
        if (toUser == null)
        {
            throw new InvalidOperationException($"User with ID {toUserId} does not exist.");
        }

        // Verifică dacă există deja o cerere pending
        var existingRequest = await _friendRequestRepository.GetByUsersAsync(fromUserId, toUserId, cancellationToken);
        if (existingRequest != null)
        {
            if (existingRequest.Status == FriendRequestStatus.Pending)
            {
                throw new InvalidOperationException("Există deja o cerere de prietenie pending între acești useri.");
            }
            if (existingRequest.Status == FriendRequestStatus.Accepted)
            {
                throw new InvalidOperationException("Acești useri sunt deja prieteni.");
            }
        }

        // Verifică dacă există deja o cerere în direcția inversă
        var reverseRequest = await _friendRequestRepository.GetByUsersAsync(toUserId, fromUserId, cancellationToken);
        if (reverseRequest != null && reverseRequest.Status == FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException("Există deja o cerere de prietenie pending de la acest user către tine.");
        }

        // Verifică dacă sunt deja prieteni
        if (await _friendRequestRepository.AreFriendsAsync(fromUserId, toUserId, cancellationToken))
        {
            throw new InvalidOperationException("Acești useri sunt deja prieteni.");
        }

        // Creează cererea
        var friendRequest = new FriendRequest
        {
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Status = FriendRequestStatus.Pending
        };

        await _friendRequestRepository.AddAsync(friendRequest, cancellationToken);
        await _friendRequestRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FriendRequestDto>(friendRequest);
    }

    public async Task<FriendRequestDto> SendFriendRequestByNameAsync(Guid fromUserId, string toUserName, CancellationToken cancellationToken = default)
    {
        // Găsește user-ul destinatar după nume
        var toUser = await _userRepository.GetByNameAsync(toUserName, cancellationToken);
        if (toUser == null)
        {
            throw new InvalidOperationException($"User cu numele '{toUserName}' nu a fost găsit.");
        }

        // Folosește metoda existentă cu validările deja implementate
        return await SendFriendRequestAsync(fromUserId, toUser.Id, cancellationToken);
    }

    public async Task<FriendRequestDto> AcceptFriendRequestAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default)
    {
        var request = await _friendRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
        {
            throw new InvalidOperationException($"Friend request with ID {requestId} does not exist.");
        }

        // Verifică că user-ul este destinatarul cererii
        if (request.ToUserId != userId)
        {
            throw new InvalidOperationException("Nu poți accepta o cerere de prietenie care nu ți-a fost trimisă.");
        }

        if (request.Status != FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Friend request is not pending. Current status: {request.Status}");
        }

        // Acceptă cererea
        request.Status = FriendRequestStatus.Accepted;
        request.UpdatedAt = DateTime.UtcNow;
        await _friendRequestRepository.UpdateAsync(request, cancellationToken);
        await _friendRequestRepository.SaveChangesAsync(cancellationToken);

        // Creează cererea inversă pentru relație bidirecțională
        // Verifică dacă nu există deja o cerere inversă
        var reverseRequest = await _friendRequestRepository.GetByUsersAsync(request.ToUserId, request.FromUserId, cancellationToken);
        if (reverseRequest == null)
        {
            var bidirectionalRequest = new FriendRequest
            {
                FromUserId = request.ToUserId,
                ToUserId = request.FromUserId,
                Status = FriendRequestStatus.Accepted
            };
            await _friendRequestRepository.AddAsync(bidirectionalRequest, cancellationToken);
            await _friendRequestRepository.SaveChangesAsync(cancellationToken);
        }
        else if (reverseRequest.Status != FriendRequestStatus.Accepted)
        {
            // Dacă există o cerere inversă care nu este Accepted, o actualizează
            reverseRequest.Status = FriendRequestStatus.Accepted;
            reverseRequest.UpdatedAt = DateTime.UtcNow;
            await _friendRequestRepository.UpdateAsync(reverseRequest, cancellationToken);
            await _friendRequestRepository.SaveChangesAsync(cancellationToken);
        }

        return _mapper.Map<FriendRequestDto>(request);
    }

    public async Task<bool> DeclineFriendRequestAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default)
    {
        var request = await _friendRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
        {
            throw new InvalidOperationException($"Friend request with ID {requestId} does not exist.");
        }

        if (request.ToUserId != userId)
        {
            throw new InvalidOperationException("Nu poți refuza o cerere de prietenie care nu ți-a fost trimisă.");
        }

        if (request.Status != FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Friend request is not pending. Current status: {request.Status}");
        }

        request.Status = FriendRequestStatus.Declined;
        request.UpdatedAt = DateTime.UtcNow;
        await _friendRequestRepository.UpdateAsync(request, cancellationToken);
        await _friendRequestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CancelFriendRequestAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default)
    {
        var request = await _friendRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null)
        {
            throw new InvalidOperationException($"Friend request with ID {requestId} does not exist.");
        }

        if (request.FromUserId != userId)
        {
            throw new InvalidOperationException("Nu poți anula o cerere de prietenie pe care nu ai trimis-o.");
        }

        if (request.Status != FriendRequestStatus.Pending)
        {
            throw new InvalidOperationException($"Friend request is not pending. Current status: {request.Status}");
        }

        await _friendRequestRepository.DeleteAsync(request, cancellationToken);
        await _friendRequestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var requests = await _friendRequestRepository.GetPendingRequestsForUserAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<FriendRequestDto>>(requests);
    }

    public async Task<IEnumerable<FriendRequestDto>> GetSentRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var requests = await _friendRequestRepository.GetSentRequestsByUserAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<FriendRequestDto>>(requests);
    }

    public async Task<IEnumerable<FriendDto>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var friendRequests = await _friendRequestRepository.GetFriendsAsync(userId, cancellationToken);
        
        var friends = new List<FriendDto>();
        foreach (var request in friendRequests)
        {
            // Determină care user este prietenul (cel care nu este userId)
            var friendUser = request.FromUserId == userId ? request.ToUser : request.FromUser;
            
            // Calculează IsOnline pe baza LastActiveAt (online dacă activ în ultimele 2 minute)
            var isOnline = friendUser.LastActiveAt.HasValue && 
                          (DateTime.UtcNow - friendUser.LastActiveAt.Value).TotalMinutes <= 2;
            
            friends.Add(new FriendDto
            {
                Id = friendUser.Id,
                Name = friendUser.Name,
                IsOnline = isOnline
            });
        }

        // Elimină duplicate-urile (în cazul în care există request-uri în ambele direcții)
        return friends
            .GroupBy(f => f.Id)
            .Select(g => g.First())
            .OrderBy(f => f.Name);
    }

    public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        return await _friendRequestRepository.AreFriendsAsync(userId1, userId2, cancellationToken);
    }

    public async Task<bool> RemoveFriendAsync(Guid userId, Guid friendId, CancellationToken cancellationToken = default)
    {
        // Găsește toate request-urile Accepted între cei doi useri
        var requests = await _friendRequestRepository.FindAsync(
            fr => fr.Status == FriendRequestStatus.Accepted &&
                  ((fr.FromUserId == userId && fr.ToUserId == friendId) ||
                   (fr.FromUserId == friendId && fr.ToUserId == userId)),
            cancellationToken);

        foreach (var request in requests)
        {
            await _friendRequestRepository.DeleteAsync(request, cancellationToken);
        }

        await _friendRequestRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FriendProfileDto> GetFriendProfileAsync(Guid userId, Guid friendId, CancellationToken cancellationToken = default)
    {
        // Verifică dacă sunt prieteni
        var areFriends = await _friendRequestRepository.AreFriendsAsync(userId, friendId, cancellationToken);
        if (!areFriends)
        {
            throw new InvalidOperationException("Acești useri nu sunt prieteni.");
        }

        // Obține informațiile despre prieten
        var friend = await _userRepository.GetByIdAsync(friendId, cancellationToken);
        if (friend == null)
        {
            throw new InvalidOperationException($"User with ID {friendId} does not exist.");
        }

        // Calculează IsOnline pe baza LastActiveAt (online dacă activ în ultimele 2 minute)
        var isOnline = friend.LastActiveAt.HasValue && 
                      (DateTime.UtcNow - friend.LastActiveAt.Value).TotalMinutes <= 2;

        // Obține participările prietenului
        var participations = await _participationRepository.GetByUserIdAsync(friendId, cancellationToken);
        var participationList = participations.ToList();

        // Obține challenge-urile la care participă prietenul
        var challengeIds = participationList.Select(p => p.ChallengeId).Distinct().ToList();
        var challenges = new List<ChallengeDto>();
        
        foreach (var challengeId in challengeIds)
        {
            var challenge = await _challengeRepository.GetByIdAsync(challengeId, cancellationToken);
            if (challenge != null)
            {
                challenges.Add(_mapper.Map<ChallengeDto>(challenge));
            }
        }

        // Calculează statisticile
        var totalChallenges = challenges.Count;
        var activeChallenges = challenges.Count(c => c.Status == ChallengeStatus.Active);
        var completedChallenges = challenges.Count(c => c.Status == ChallengeStatus.Completed);
        
        // Calculează victoriile (challenge-uri completate unde prietenul are cel mai mare scor)
        var victories = 0;
        var totalScore = participationList.Sum(p => p.TotalScore);
        
        foreach (var challenge in challenges.Where(c => c.Status == ChallengeStatus.Completed))
        {
            var challengeParticipations = await _participationRepository.GetByChallengeIdAsync(challenge.Id, cancellationToken);
            var participationListForChallenge = challengeParticipations.ToList();
            
            if (participationListForChallenge.Any())
            {
                var maxScore = participationListForChallenge.Max(p => p.TotalScore);
                var friendParticipation = participationListForChallenge.FirstOrDefault(p => p.UserId == friendId);
                
                if (friendParticipation != null && friendParticipation.TotalScore == maxScore)
                {
                    victories++;
                }
            }
        }

        return new FriendProfileDto
        {
            Id = friend.Id,
            Name = friend.Name,
            IsOnline = isOnline,
            Challenges = challenges,
            Statistics = new FriendStatisticsDto
            {
                TotalChallenges = totalChallenges,
                ActiveChallenges = activeChallenges,
                CompletedChallenges = completedChallenges,
                Victories = victories,
                TotalScore = (int)totalScore
            }
        };
    }
}

