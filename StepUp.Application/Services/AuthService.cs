using AutoMapper;
using StepUp.Application.DTOs.Auth;
using StepUp.Application.DTOs.User;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IPasswordService passwordService,
        IEmailVerificationService emailVerificationService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _emailVerificationService = emailVerificationService;
        _mapper = mapper;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        // Validări explicite pentru câmpurile obligatorii
        if (string.IsNullOrWhiteSpace(registerDto.Name))
        {
            throw new ArgumentException("Numele este obligatoriu.");
        }

        if (string.IsNullOrWhiteSpace(registerDto.Email))
        {
            throw new ArgumentException("Email-ul este obligatoriu.");
        }

        if (string.IsNullOrWhiteSpace(registerDto.Password))
        {
            throw new ArgumentException("Parola este obligatorie.");
        }

        if (registerDto.Password.Length < 6)
        {
            throw new ArgumentException("Parola trebuie să aibă cel puțin 6 caractere.");
        }

        // Verifică dacă emailul există deja
        var existingUser = await _userRepository.GetByEmailAsync(registerDto.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Acest email este deja înregistrat. Te rog folosește alt email sau conectează-te.");
        }

        // Hash-uiește parola
        var passwordHash = _passwordService.HashPassword(registerDto.Password);

        // Creează user-ul cu parola hash-uită
        var user = new User
        {
            Name = registerDto.Name,
            Email = registerDto.Email,
            PasswordHash = passwordHash,
            Role = Role.User, // Default role pentru utilizatori noi
            HeightCm = registerDto.HeightCm,
            WeightKg = registerDto.WeightKg,
            Age = registerDto.Age,
            Gender = registerDto.Gender,
            DailyStepsGoal = registerDto.DailyStepsGoal ?? 10000
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Trimite email de verificare
        try
        {
            await _emailVerificationService.ResendVerificationEmailAsync(user.Email, cancellationToken);
        }
        catch
        {
            // Nu aruncăm eroare dacă email-ul nu poate fi trimis, utilizatorul poate cere retrimiterea mai târziu
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        // Verifică dacă emailul există
        var user = await _userRepository.GetByEmailAsync(loginDto.Email, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        // Verifică parola
        var isPasswordValid = _passwordService.VerifyPassword(loginDto.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        if (user.IsSuspended)
        {
            throw new InvalidOperationException("Contul este suspendat.");
        }

        // Verifică dacă email-ul este verificat
        if (!user.IsEmailVerified)
        {
            throw new InvalidOperationException("Te rugăm să verifici email-ul înainte de a te conecta. Verifică inbox-ul pentru link-ul de verificare.");
        }

        // Setează LastActiveAt imediat la login pentru a fi considerat online instant
        user.LastActiveAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> CheckEmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        return user != null;
    }
}

