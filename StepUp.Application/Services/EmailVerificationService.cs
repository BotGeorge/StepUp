using Microsoft.Extensions.Configuration;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using System.Security.Cryptography;

namespace StepUp.Application.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IEmailVerificationTokenRepository _tokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EmailVerificationService(
        IEmailVerificationTokenRepository tokenRepository,
        IUserRepository userRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _tokenRepository = tokenRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<string> GenerateVerificationTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Șterge token-urile vechi pentru acest user
        var oldTokens = (await _tokenRepository.GetByUserIdAsync(userId, cancellationToken))
            .Where(t => !t.IsUsed)
            .ToList();

        foreach (var oldToken in oldTokens)
        {
            await _tokenRepository.DeleteAsync(oldToken, cancellationToken);
        }

        // Generează un token nou
        var tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var verificationToken = new EmailVerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // Expiră în 24 de ore
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _tokenRepository.AddAsync(verificationToken, cancellationToken);
        await _tokenRepository.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var verificationToken = await _tokenRepository.GetByTokenAsync(token, cancellationToken);
        
        if (verificationToken == null || verificationToken.IsUsed)
        {
            return false; // Token invalid sau deja folosit
        }

        if (verificationToken == null)
        {
            return false; // Token invalid sau deja folosit
        }

        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            return false; // Token expirat
        }

        // Marchează token-ul ca folosit
        verificationToken.IsUsed = true;
        verificationToken.UpdatedAt = DateTime.UtcNow;

        // Marchează email-ul utilizatorului ca verificat
        var user = verificationToken.User;
        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _tokenRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("Utilizatorul nu a fost găsit.");
        }

        if (user.IsEmailVerified)
        {
            throw new InvalidOperationException("Email-ul este deja verificat.");
        }

        var token = await GenerateVerificationTokenAsync(user.Id, cancellationToken);
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5205";
        var verificationLink = $"{baseUrl}/api/auth/verify-email?token={token}";

        await _emailService.SendVerificationEmailAsync(user.Email, user.Name, verificationLink, cancellationToken);
    }
}
