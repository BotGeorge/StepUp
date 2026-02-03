using StepUp.Application.DTOs.Friend;

namespace StepUp.Application.Interfaces;

public interface IFriendService
{
    Task<FriendRequestDto> SendFriendRequestAsync(Guid fromUserId, Guid toUserId, CancellationToken cancellationToken = default);
    Task<FriendRequestDto> SendFriendRequestByNameAsync(Guid fromUserId, string toUserName, CancellationToken cancellationToken = default);
    Task<FriendRequestDto> AcceptFriendRequestAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeclineFriendRequestAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CancelFriendRequestAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendRequestDto>> GetPendingRequestsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendRequestDto>> GetSentRequestsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendDto>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<FriendProfileDto> GetFriendProfileAsync(Guid userId, Guid friendId, CancellationToken cancellationToken = default);
    Task<bool> AreFriendsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<bool> RemoveFriendAsync(Guid userId, Guid friendId, CancellationToken cancellationToken = default);
}

