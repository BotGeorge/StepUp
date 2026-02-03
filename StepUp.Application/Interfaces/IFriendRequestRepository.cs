using StepUp.Domain.Entities;
using StepUp.Domain.Enums;
using System.Linq.Expressions;

namespace StepUp.Application.Interfaces;

public interface IFriendRequestRepository
{
    Task<FriendRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendRequest>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendRequest>> FindAsync(Expression<Func<FriendRequest, bool>> predicate, CancellationToken cancellationToken = default);
    Task<FriendRequest> AddAsync(FriendRequest entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(FriendRequest entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(FriendRequest entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FriendRequest?> GetByUsersAsync(Guid fromUserId, Guid toUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendRequest>> GetPendingRequestsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendRequest>> GetSentRequestsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<FriendRequest>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AreFriendsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default);
    Task<bool> HasPendingRequestAsync(Guid fromUserId, Guid toUserId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

