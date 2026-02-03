using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.User;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime? LastActiveAt { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public int? Age { get; set; }
    public Gender Gender { get; set; } = Gender.Unspecified;
    public int DailyStepsGoal { get; set; } = 10000;
}

