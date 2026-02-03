using StepUp.Application.DTOs.Auth;
using StepUp.Application.DTOs.User;

namespace StepUp.Application.Interfaces;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<UserDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default);
}

