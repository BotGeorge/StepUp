using Microsoft.EntityFrameworkCore;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Infrastructure.Data;

namespace StepUp.Infrastructure.Repositories;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly ApplicationDbContext _context;

    public EmailVerificationTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task<IEnumerable<EmailVerificationToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailVerificationToken> AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        token.CreatedAt = DateTime.UtcNow;
        await _context.EmailVerificationTokens.AddAsync(token, cancellationToken);
        return token;
    }

    public async Task DeleteAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
    {
        _context.EmailVerificationTokens.Remove(token);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
