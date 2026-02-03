using StepUp.Domain.Entities;

namespace StepUp.Application.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetAllWithUserAsync(CancellationToken cancellationToken = default);
    Task<Post?> GetByIdWithUserAndCommentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Post>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Post>> GetByFriendIdsAsync(IEnumerable<Guid> friendIds, CancellationToken cancellationToken = default);
    Task<IEnumerable<Post>> GetByFriendIdsAndRolesAsync(IEnumerable<Guid> friendIds, IEnumerable<int> roles, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

