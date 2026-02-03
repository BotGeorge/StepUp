namespace StepUp.Application.Interfaces;

public interface IEmailVerificationService
{
    Task<string> GenerateVerificationTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
    Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);
}
