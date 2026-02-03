using AutoMapper;
using StepUp.Application.DTOs.ActivityLog;
using StepUp.Application.DTOs.Challenge;
using StepUp.Application.DTOs.Comment;
using StepUp.Application.DTOs.Friend;
using StepUp.Application.DTOs.Notification;
using StepUp.Application.DTOs.Participation;
using StepUp.Application.DTOs.Post;
using StepUp.Application.DTOs.User;
using StepUp.Domain.Entities;

namespace StepUp.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => string.Empty)); // Default empty, will be set by service if needed
        CreateMap<User, UserDto>();

        // Challenge mappings
        CreateMap<CreateChallengeDto, Challenge>();
        CreateMap<Challenge, ChallengeDto>()
            .ForMember(dest => dest.SponsorName, opt => opt.MapFrom(src => src.Sponsor != null ? src.Sponsor.Name : null))
            .ForMember(dest => dest.Prize, opt => opt.MapFrom(src => src.IsSponsored ? src.Prize : null)) // Prize doar dacă IsSponsored = true
            .ForMember(dest => dest.WinnerName, opt => opt.MapFrom(src => src.WinnerUser != null ? src.WinnerUser.Name : null))
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.Name : null));

        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)src.Type));

        // Participation mappings
        CreateMap<Participation, ParticipationDto>();

        // ActivityLog mappings
        CreateMap<CreateActivityLogDto, ActivityLog>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => 
                src.Date.Kind == DateTimeKind.Utc 
                    ? src.Date 
                    : DateTime.SpecifyKind(src.Date, DateTimeKind.Utc)));
        CreateMap<ActivityLog, ActivityLogDto>();

        // FriendRequest mappings
        CreateMap<FriendRequest, FriendRequestDto>()
            .ForMember(dest => dest.FromUserName, opt => opt.MapFrom(src => src.FromUser.Name))
            .ForMember(dest => dest.ToUserName, opt => opt.MapFrom(src => src.ToUser.Name));

        // Post mappings
        CreateMap<CreatePostDto, Post>();
        CreateMap<Post, PostDto>();

        // Comment mappings
        CreateMap<CreateCommentDto, Comment>();
        CreateMap<Comment, CommentDto>();
    }
}

