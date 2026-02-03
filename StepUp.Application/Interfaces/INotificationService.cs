using StepUp.Application.DTOs.Notification;

namespace StepUp.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task CreateChallengeCompletionNotificationsAsync(Guid challengeId, string challengeName, Guid winnerUserId, IEnumerable<Guid> participantUserIds, CancellationToken cancellationToken = default);
}
