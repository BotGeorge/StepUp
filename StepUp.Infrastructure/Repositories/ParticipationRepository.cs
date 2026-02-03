using Microsoft.EntityFrameworkCore;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Infrastructure.Data;

namespace StepUp.Infrastructure.Repositories;

public class ParticipationRepository : IParticipationRepository
{
    private readonly ApplicationDbContext _context;

    public ParticipationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Participation> AddAsync(Participation participation, CancellationToken cancellationToken = default)
    {
        participation.CreatedAt = DateTime.UtcNow;
        await _context.Participations.AddAsync(participation, cancellationToken);
        return participation;
    }

    public async Task<Participation?> GetByUserAndChallengeAsync(Guid userId, Guid challengeId, CancellationToken cancellationToken = default)
    {
        return await _context.Participations
            .FirstOrDefaultAsync(p => p.UserId == userId && p.ChallengeId == challengeId, cancellationToken);
    }

    public async Task<IEnumerable<Participation>> GetByChallengeIdAsync(Guid challengeId, CancellationToken cancellationToken = default)
    {
        return await _context.Participations
            .Where(p => p.ChallengeId == challengeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Participation>> GetByChallengeIdWithUserAsync(Guid challengeId, CancellationToken cancellationToken = default)
    {
        return await _context.Participations
            .Include(p => p.User)
            .Where(p => p.ChallengeId == challengeId)
            .OrderByDescending(p => p.TotalScore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Participation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Participations
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid challengeId, CancellationToken cancellationToken = default)
    {
        return await _context.Participations
            .AnyAsync(p => p.UserId == userId && p.ChallengeId == challengeId, cancellationToken);
    }

    public Task UpdateAsync(Participation participation, CancellationToken cancellationToken = default)
    {
        participation.UpdatedAt = DateTime.UtcNow;
        _context.Participations.Update(participation);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

