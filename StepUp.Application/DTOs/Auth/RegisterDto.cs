using System.ComponentModel.DataAnnotations;

namespace StepUp.Application.DTOs.Auth;

public class RegisterDto
{
    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Numele trebuie să aibă între 1 și 200 de caractere.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email-ul este obligatoriu.")]
    [EmailAddress(ErrorMessage = "Email-ul nu este valid.")]
    [StringLength(255, ErrorMessage = "Email-ul nu poate depăși 255 de caractere.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere.")]
    public string Password { get; set; } = string.Empty;

    public int? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public int? Age { get; set; }
    public Domain.Enums.Gender Gender { get; set; } = Domain.Enums.Gender.Unspecified;
    public int? DailyStepsGoal { get; set; }
}

