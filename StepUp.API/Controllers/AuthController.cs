using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StepUp.Application.DTOs.Auth;
using StepUp.Application.Interfaces;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService, 
        IEmailVerificationService emailVerificationService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _emailVerificationService = emailVerificationService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
    {
        // Validare ModelState (pentru Data Annotations)
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = "Date invalide.",
                errors = errors
            });
        }

        try
        {
            var userDto = await _authService.RegisterAsync(registerDto, cancellationToken);
            
            return CreatedAtAction(
                nameof(Register), 
                new { id = userDto.Id }, 
                new
                {
                    success = true,
                    message = "User registered successfully.",
                    user = userDto
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            // Nu loga erorile așteptate (email deja existent)
            var lowerMsg = ex.Message?.ToLower() ?? "";
            var isExpectedError = 
                lowerMsg.Contains("deja înregistrat") ||
                lowerMsg.Contains("already exists");
            
            if (!isExpectedError)
            {
                _logger.LogWarning("Registration failed: {Message}", ex.Message);
            }
            
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while registering the user.",
                error = ex.Message
            });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        // Validare ModelState (pentru Data Annotations)
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = "Date invalide.",
                errors = errors
            });
        }

        try
        {
            var userDto = await _authService.LoginAsync(loginDto, cancellationToken);
            
            return Ok(new
            {
                success = true,
                message = "Login successful.",
                user = userDto
            });
        }
        catch (InvalidOperationException ex)
        {
            // Nu loga erorile așteptate (email neverificat, credențiale greșite, cont suspendat)
            var lowerMsg = ex.Message?.ToLower() ?? "";
            var isExpectedError = 
                lowerMsg.Contains("verifici email") ||
                lowerMsg.Contains("verifică email") ||
                lowerMsg.Contains("email verificat") ||
                lowerMsg.Contains("email-ul nu este verificat") ||
                lowerMsg.Contains("email nu este verificat") ||
                lowerMsg.Contains("invalid email") ||
                lowerMsg.Contains("invalid password") ||
                lowerMsg.Contains("suspendat");
            
            if (!isExpectedError)
            {
                _logger.LogWarning("Login failed: {Message}", ex.Message);
            }
            
            return Unauthorized(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while logging in.",
                error = ex.Message
            });
        }
    }

    [HttpGet("check-email")]
    public async Task<ActionResult> CheckEmailExists([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                success = false,
                message = "Email-ul este obligatoriu."
            });
        }

        try
        {
            var exists = await _authService.CheckEmailExistsAsync(email, cancellationToken);
            return Ok(new
            {
                success = true,
                exists = exists,
                message = exists ? "Acest email este deja înregistrat." : "Email disponibil."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la verificarea email-ului.",
                error = ex.Message
            });
        }
    }

    [HttpGet("verify-email")]
    public async Task<ActionResult> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Content(GetErrorHtml("Token-ul este obligatoriu."), "text/html");
        }

        try
        {
            var verified = await _emailVerificationService.VerifyEmailAsync(token, cancellationToken);
            
            if (verified)
            {
                return Content(GetSuccessHtml(), "text/html");
            }
            else
            {
                return Content(GetErrorHtml("Token invalid sau expirat. Te rog solicită un nou email de verificare."), "text/html");
            }
        }
        catch (Exception)
        {
            return Content(GetErrorHtml("A apărut o eroare la verificarea email-ului."), "text/html");
        }
    }

    private string GetSuccessHtml()
    {
        return @"
<!DOCTYPE html>
<html lang=""ro"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Verificat - StepUp</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #000000 0%, #1a1a1a 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }
        .container {
            background: rgba(255, 255, 255, 0.05);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 40px;
            max-width: 500px;
            width: 100%;
            text-align: center;
            border: 1px solid rgba(52, 199, 89, 0.3);
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
        }
        .success-icon {
            width: 80px;
            height: 80px;
            margin: 0 auto 30px;
            background: #34C759;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            animation: scaleIn 0.5s ease-out;
        }
        .success-icon::after {
            content: '✓';
            color: white;
            font-size: 50px;
            font-weight: bold;
        }
        @keyframes scaleIn {
            from {
                transform: scale(0);
                opacity: 0;
            }
            to {
                transform: scale(1);
                opacity: 1;
            }
        }
        h1 {
            color: #34C759;
            font-size: 28px;
            margin-bottom: 15px;
            font-weight: 700;
        }
        .message {
            color: rgba(255, 255, 255, 0.9);
            font-size: 18px;
            line-height: 1.6;
            margin-bottom: 30px;
        }
        .submessage {
            color: rgba(255, 255, 255, 0.7);
            font-size: 14px;
            margin-top: 20px;
        }
        .button {
            display: inline-block;
            background: #34C759;
            color: white;
            padding: 14px 30px;
            border-radius: 8px;
            text-decoration: none;
            font-weight: 600;
            font-size: 16px;
            transition: all 0.3s ease;
            margin-top: 10px;
        }
        .button:hover {
            background: #2fb04a;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(52, 199, 89, 0.4);
        }
        .logo {
            margin-bottom: 30px;
            font-size: 32px;
            font-weight: bold;
            color: #34C759;
        }
        .instruction-box {
            background: rgba(52, 199, 89, 0.1);
            border: 1px solid rgba(52, 199, 89, 0.3);
            border-radius: 12px;
            padding: 20px;
            margin-top: 20px;
        }
        .instruction-title {
            color: #34C759;
            font-size: 16px;
            font-weight: 600;
            margin-bottom: 10px;
        }
        .instruction-text {
            color: rgba(255, 255, 255, 0.8);
            font-size: 14px;
            line-height: 1.6;
        }
        .step {
            margin-bottom: 8px;
        }
        .step-number {
            color: #34C759;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""logo"">⚡ StepUp</div>
        <div class=""success-icon""></div>
        <h1>Email Verificat!</h1>
        <p class=""message"">
            Contul tău a fost verificat cu succes!<br>
            Acum poți folosi toate funcționalitățile aplicației.
        </p>
        
        <div class=""instruction-box"">
            <div class=""instruction-title"">📱 Pași următori:</div>
            <div class=""instruction-text"">
                <div class=""step"">
                    <span class=""step-number"">1.</span> Închide această pagină
                </div>
                <div class=""step"">
                    <span class=""step-number"">2.</span> Deschide aplicația StepUp
                </div>
                <div class=""step"">
                    <span class=""step-number"">3.</span> Conectează-te cu contul tău
                </div>
            </div>
        </div>
        
        <p class=""submessage"">
            Aplicația va detecta automat că email-ul tău a fost verificat.
        </p>
    </div>
</body>
</html>";
    }

    private string GetErrorHtml(string errorMessage)
    {
        return $@"
<!DOCTYPE html>
<html lang=""ro"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Eroare Verificare - StepUp</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            background: linear-gradient(135deg, #000000 0%, #1a1a1a 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            padding: 20px;
        }}
        .container {{
            background: rgba(255, 255, 255, 0.05);
            backdrop-filter: blur(10px);
            border-radius: 20px;
            padding: 40px;
            max-width: 500px;
            width: 100%;
            text-align: center;
            border: 1px solid rgba(255, 59, 48, 0.3);
            box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
        }}
        .error-icon {{
            width: 80px;
            height: 80px;
            margin: 0 auto 30px;
            background: #FF3B30;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
        }}
        .error-icon::after {{
            content: '✗';
            color: white;
            font-size: 50px;
            font-weight: bold;
        }}
        h1 {{
            color: #FF3B30;
            font-size: 28px;
            margin-bottom: 15px;
            font-weight: 700;
        }}
        .message {{
            color: rgba(255, 255, 255, 0.9);
            font-size: 18px;
            line-height: 1.6;
            margin-bottom: 30px;
        }}
        .submessage {{
            color: rgba(255, 255, 255, 0.7);
            font-size: 14px;
            margin-top: 20px;
        }}
        .logo {{
            margin-bottom: 30px;
            font-size: 32px;
            font-weight: bold;
            color: #34C759;
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""logo"">⚡ StepUp</div>
        <div class=""error-icon""></div>
        <h1>Eroare la Verificare</h1>
        <p class=""message"">
            {errorMessage}
        </p>
        <p class=""submessage"">
            Te rugăm să verifici link-ul sau să soliciți un nou email de verificare din aplicație.
        </p>
    </div>
</body>
</html>";
    }

    [HttpPost("resend-verification-email")]
    public async Task<ActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new
            {
                success = false,
                message = "Email-ul este obligatoriu."
            });
        }

        try
        {
            await _emailVerificationService.ResendVerificationEmailAsync(dto.Email, cancellationToken);
            
            return Ok(new
            {
                success = true,
                message = "Email-ul de verificare a fost trimis cu succes!"
            });
        }
        catch (InvalidOperationException ex)
        {
            // Nu loga erorile așteptate (utilizator nu găsit, email deja verificat)
            var lowerMsg = ex.Message?.ToLower() ?? "";
            var isExpectedError = 
                lowerMsg.Contains("nu a fost găsit") ||
                lowerMsg.Contains("not found") ||
                lowerMsg.Contains("deja verificat") ||
                lowerMsg.Contains("already verified");
            
            if (!isExpectedError)
            {
                _logger.LogWarning("Resend verification email failed: {Message}", ex.Message);
            }
            
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while resending verification email");
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la trimiterea email-ului de verificare.",
                error = ex.Message
            });
        }
    }
}

public class ResendVerificationEmailDto
{
    public string Email { get; set; } = string.Empty;
}

