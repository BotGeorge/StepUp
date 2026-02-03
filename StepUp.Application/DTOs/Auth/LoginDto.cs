using System.ComponentModel.DataAnnotations;

namespace StepUp.Application.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "Email-ul este obligatoriu.")]
    [EmailAddress(ErrorMessage = "Email-ul nu este valid.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie.")]
    public string Password { get; set; } = string.Empty;
}

