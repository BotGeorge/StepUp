using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using StepUp.Application.Interfaces;

namespace StepUp.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPortStr = _configuration["Email:SmtpPort"];
        var smtpPort = int.TryParse(smtpPortStr, out var port) ? port : 587;
        var smtpUsername = _configuration["Email:SmtpUsername"];
        var smtpPassword = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@stepup.com";
        var fromName = _configuration["Email:FromName"] ?? "StepUp";

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUsername) || string.IsNullOrWhiteSpace(smtpPassword))
        {
            _logger.LogWarning("Email configuration is missing. Email will not be sent.");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpUsername, smtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            // Nu loga erorile de trimitere email ca erori critice - sunt așteptate în unele cazuri
            _logger.LogWarning(ex, "Failed to send email to {Email}. User can request resend.", to);
            throw;
        }
    }

    public async Task SendVerificationEmailAsync(string to, string name, string verificationLink, CancellationToken cancellationToken = default)
    {
        var subject = "Verifică email-ul tău - StepUp";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #34C759; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 30px; background-color: #34C759; color: white; text-decoration: none; border-radius: 6px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Bun venit la StepUp!</h1>
        </div>
        <div class='content'>
            <p>Salut {name},</p>
            <p>Mulțumim că te-ai înregistrat! Pentru a-ți activa contul, te rugăm să verifici adresa ta de email.</p>
            <p style='text-align: center;'>
                <a href='{verificationLink}' class='button'>Verifică Email-ul</a>
            </p>
            <p>Dacă butonul nu funcționează, copiază și lipește următorul link în browser:</p>
            <p style='word-break: break-all; color: #34C759;'>{verificationLink}</p>
            <p>Acest link expiră în 24 de ore.</p>
            <p>Dacă nu ai creat acest cont, te rugăm să ignori acest email.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} StepUp. Toate drepturile rezervate.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(to, subject, body, cancellationToken);
    }
}
