namespace StepUp.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendVerificationEmailAsync(string to, string name, string verificationLink, CancellationToken cancellationToken = default);
}
