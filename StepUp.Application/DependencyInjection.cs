using Microsoft.Extensions.DependencyInjection;
using StepUp.Application.Interfaces;
using StepUp.Application.Mappings;
using StepUp.Application.Services;

namespace StepUp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // Register application services
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IChallengeService, ChallengeService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IFriendService, FriendService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailVerificationService, EmailVerificationService>();

        return services;
    }
}

