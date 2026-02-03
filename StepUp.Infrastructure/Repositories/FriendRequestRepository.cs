using Microsoft.EntityFrameworkCore;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;
using StepUp.Infrastructure.Data;

namespace StepUp.Infrastructure.Repositories;

public class FriendRequestRepository : Repository<FriendRequest>, IFriendRequestRepository
{
    public FriendRequestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<FriendRequest?> GetByUsersAsync(Guid fromUserId, Guid toUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(fr => fr.FromUser)
            .Include(fr => fr.ToUser)
            .FirstOrDefaultAsync(fr => fr.FromUserId == fromUserId && fr.ToUserId == toUserId, cancellationToken);
    }

    public async Task<IEnumerable<FriendRequest>> GetPendingRequestsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(fr => fr.FromUser)
            .Where(fr => fr.ToUserId == userId && fr.Status == FriendRequestStatus.Pending)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FriendRequest>> GetSentRequestsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(fr => fr.ToUser)
            .Where(fr => fr.FromUserId == userId)
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FriendRequest>> GetFriendsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Returnează toate request-urile Accepted unde user-ul este FromUser sau ToUser
        return await _dbSet
            .Include(fr => fr.FromUser)
            .Include(fr => fr.ToUser)
            .Where(fr => fr.Status == FriendRequestStatus.Accepted && 
                       (fr.FromUserId == userId || fr.ToUserId == userId))
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AreFriendsAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        // Verifică dacă există un request Accepted între cei doi useri (în orice direcție)
        return await _dbSet
            .AnyAsync(fr => 
                fr.Status == FriendRequestStatus.Accepted &&
                ((fr.FromUserId == userId1 && fr.ToUserId == userId2) ||
                 (fr.FromUserId == userId2 && fr.ToUserId == userId1)),
                cancellationToken);
    }

    public async Task<bool> HasPendingRequestAsync(Guid fromUserId, Guid toUserId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(fr => 
                fr.Status == FriendRequestStatus.Pending &&
                fr.FromUserId == fromUserId && 
                fr.ToUserId == toUserId,
                cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

