using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.User;

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.User;
}

