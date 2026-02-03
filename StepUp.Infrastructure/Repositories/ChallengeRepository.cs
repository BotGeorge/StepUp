using Microsoft.EntityFrameworkCore;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;
using StepUp.Infrastructure.Data;

namespace StepUp.Infrastructure.Repositories;

public class ChallengeRepository : IChallengeRepository
{
    private readonly ApplicationDbContext _context;

    public ChallengeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Challenge> AddAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        challenge.CreatedAt = DateTime.UtcNow;
        await _context.Challenges.AddAsync(challenge, cancellationToken);
        return challenge;
    }

    public async Task<IEnumerable<Challenge>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Challenges
            .Include(c => c.Sponsor)
            .Include(c => c.WinnerUser)
            .Include(c => c.CreatedByUser)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Challenge>> GetActiveChallengesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Challenges
            .Include(c => c.Sponsor)
            .Include(c => c.WinnerUser)
            .Include(c => c.CreatedByUser)
            .Where(c => (c.TargetValue.HasValue && c.TargetValue.Value > 0)
                ? (
                    (c.Status == ChallengeStatus.Active && c.StartDate <= now)
                    || (c.StartDate > now && c.Status != ChallengeStatus.Completed)
                  )
                : (
                    (c.Status == ChallengeStatus.Active 
                     && c.StartDate <= now 
                     && c.EndDate >= now)
                    || (c.StartDate > now && c.EndDate >= now)
                  )
            ) // Include future challenges
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Challenge>> GetVisibleChallengesForUserAsync(Guid userId, IEnumerable<Guid> friendIds, CancellationToken cancellationToken = default)
    {
        var friendIdsList = friendIds.ToList();
        var now = DateTime.UtcNow;
        
        return await _context.Challenges
            .Include(c => c.Sponsor)
            .Include(c => c.WinnerUser)
            .Include(c => c.CreatedByUser)
            .Where(c => 
                // Challenge-uri publice
                (c.IsPublic) ||
                // Challenge-uri făcute de utilizatorul curent
                (c.CreatedByUserId == userId) ||
                // Challenge-uri private de la prieteni
                (!c.IsPublic && c.CreatedByUserId.HasValue && friendIdsList.Contains(c.CreatedByUserId.Value))
            )
            .OrderBy(c => c.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Challenge?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Challenges
            .Include(c => c.Sponsor)
            .Include(c => c.WinnerUser)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Challenge>> GetExpiredChallengesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Challenges
            .Where(c => c.Status == ChallengeStatus.Active 
                     && (!c.TargetValue.HasValue || c.TargetValue.Value <= 0)
                     && c.EndDate < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Challenge>> GetCompletedChallengesOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        return await _context.Challenges
            .Where(c => c.Status == ChallengeStatus.Completed
                     && c.CompletedAt.HasValue
                     && c.CompletedAt.Value < cutoffUtc)
            .ToListAsync(cancellationToken);
    }

    public Task UpdateAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        challenge.UpdatedAt = DateTime.UtcNow;
        _context.Challenges.Update(challenge);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        _context.Challenges.Remove(challenge);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

