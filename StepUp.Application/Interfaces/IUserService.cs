using StepUp.Application.DTOs.User;
using StepUp.Domain.Enums;

namespace StepUp.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserRoleAsync(Guid id, Role role, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserSuspensionAsync(Guid id, bool isSuspended, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid id, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);
}

