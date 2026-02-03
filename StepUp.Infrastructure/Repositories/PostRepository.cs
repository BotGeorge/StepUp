using Microsoft.EntityFrameworkCore;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Infrastructure.Data;

namespace StepUp.Infrastructure.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Post>> GetAllWithUserAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Comments)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Post?> GetByIdWithUserAndCommentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Post>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Comments)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Post>> GetByFriendIdsAsync(IEnumerable<Guid> friendIds, CancellationToken cancellationToken = default)
    {
        var friendIdsList = friendIds.ToList();
        if (!friendIdsList.Any())
        {
            return new List<Post>();
        }

        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Comments)
            .Where(p => friendIdsList.Contains(p.UserId))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Post>> GetByFriendIdsAndRolesAsync(IEnumerable<Guid> friendIds, IEnumerable<int> roles, CancellationToken cancellationToken = default)
    {
        var friendIdsList = friendIds.ToList();
        var rolesList = roles.ToList();

        // Returnează postările de la prieteni SAU de la utilizatori cu rolurile specificate (Partner/Admin)
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Comments)
            .Where(p => (friendIdsList.Any() && friendIdsList.Contains(p.UserId)) || 
                       (p.User != null && rolesList.Contains((int)p.User.Role)))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

