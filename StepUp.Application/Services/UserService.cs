using AutoMapper;
using StepUp.Application.DTOs.User;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IPasswordService passwordService, IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _mapper = mapper;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto, CancellationToken cancellationToken = default)
    {
        var user = _mapper.Map<User>(createUserDto);
        
        // Set default empty password hash if not provided
        // In a real application, you would hash the password here using BCrypt or similar
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            user.PasswordHash = string.Empty; // Will be set when password is provided
        }
        
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
            return null;

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Verifică dacă emailul nou există deja (dacă se schimbă emailul)
        if (!string.IsNullOrEmpty(updateUserDto.Email) && updateUserDto.Email != user.Email)
        {
            var existingUser = await _userRepository.GetByEmailAsync(updateUserDto.Email, cancellationToken);
            if (existingUser != null && existingUser.Id != id)
            {
                throw new InvalidOperationException($"Email {updateUserDto.Email} is already in use.");
            }
            user.Email = updateUserDto.Email;
        }

        // Actualizează numele dacă este furnizat
        if (!string.IsNullOrEmpty(updateUserDto.Name))
        {
            user.Name = updateUserDto.Name;
        }

        // Actualizează poza de profil dacă este furnizată
        if (updateUserDto.ProfileImageUrl != null)
        {
            user.ProfileImageUrl = updateUserDto.ProfileImageUrl;
        }

        if (updateUserDto.HeightCm.HasValue)
        {
            user.HeightCm = updateUserDto.HeightCm.Value;
        }

        if (updateUserDto.WeightKg.HasValue)
        {
            user.WeightKg = updateUserDto.WeightKg.Value;
        }

        if (updateUserDto.Age.HasValue)
        {
            user.Age = updateUserDto.Age.Value;
        }

        if (updateUserDto.Gender.HasValue)
        {
            user.Gender = updateUserDto.Gender.Value;
        }

        if (updateUserDto.DailyStepsGoal.HasValue)
        {
            user.DailyStepsGoal = updateUserDto.DailyStepsGoal.Value;
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateUserRoleAsync(Guid id, Role role, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.Role = role;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateUserSuspensionAsync(Guid id, bool isSuspended, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.IsSuspended = isSuspended;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // Verifică parola curentă
        if (!_passwordService.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        // Validează parola nouă
        if (string.IsNullOrEmpty(changePasswordDto.NewPassword) || changePasswordDto.NewPassword.Length < 6)
        {
            throw new InvalidOperationException("New password must be at least 6 characters long.");
        }

        // Hash-uiește parola nouă
        user.PasswordHash = _passwordService.HashPassword(changePasswordDto.NewPassword);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}

