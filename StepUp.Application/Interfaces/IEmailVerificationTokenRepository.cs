using StepUp.Domain.Entities;

namespace StepUp.Application.Interfaces;

public interface IEmailVerificationTokenRepository
{
    Task<EmailVerificationToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<EmailVerificationToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken> AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
    Task DeleteAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
