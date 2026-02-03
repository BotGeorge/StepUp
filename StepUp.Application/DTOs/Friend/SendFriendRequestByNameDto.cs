namespace StepUp.Application.DTOs.Friend;

public class SendFriendRequestByNameDto
{
    public Guid FromUserId { get; set; }
    public string ToUserName { get; set; } = string.Empty;
}

