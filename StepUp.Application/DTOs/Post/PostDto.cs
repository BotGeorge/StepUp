namespace StepUp.Application.DTOs.Post;

public class PostDto : BaseDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UserRole { get; set; }
    public string? UserProfileImageUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int CommentCount { get; set; }
}

