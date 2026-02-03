using AutoMapper;
using StepUp.Application.DTOs.Notification;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository notificationRepository, IMapper mapper)
    {
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateChallengeCompletionNotificationsAsync(
        Guid challengeId,
        string challengeName,
        Guid winnerUserId,
        IEnumerable<Guid> participantUserIds,
        CancellationToken cancellationToken = default)
    {
        var notifications = new List<Notification>();
        foreach (var userId in participantUserIds.Distinct())
        {
            var isWinner = userId == winnerUserId;
            var title = isWinner ? "Ai câștigat!" : "Challenge încheiat";
            var message = isWinner
                ? $"Felicitări, ai câștigat challenge-ul {challengeName}."
                : $"Challenge-ul {challengeName} s-a terminat.";

            notifications.Add(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = isWinner ? NotificationType.ChallengeWon : NotificationType.ChallengeEnded,
                IsRead = false,
                ChallengeId = challengeId,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (notifications.Count == 0)
        {
            return;
        }

        await _notificationRepository.AddRangeAsync(notifications, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }
}
