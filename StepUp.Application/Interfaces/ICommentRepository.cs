using StepUp.Domain.Entities;

namespace StepUp.Application.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByPostIdWithUserAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Comment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

