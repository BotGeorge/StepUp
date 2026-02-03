namespace StepUp.Application.DTOs.User;

public class UpdateUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ProfileImageUrl { get; set; }
    public int? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public int? Age { get; set; }
    public Domain.Enums.Gender? Gender { get; set; }
    public int? DailyStepsGoal { get; set; }
}
