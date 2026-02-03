using StepUp.Domain.Enums;

namespace StepUp.Application.DTOs.Friend;

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public Guid FromUserId { get; set; }
    public string FromUserName { get; set; } = string.Empty;
    public Guid ToUserId { get; set; }
    public string ToUserName { get; set; } = string.Empty;
    public FriendRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

