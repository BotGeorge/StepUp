namespace StepUp.Application.DTOs.Comment;

public class CommentDto : BaseDto
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UserRole { get; set; }
    public string? UserProfileImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
}

