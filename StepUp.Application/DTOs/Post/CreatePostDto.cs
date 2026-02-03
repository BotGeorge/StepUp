namespace StepUp.Application.DTOs.Post;

public class CreatePostDto
{
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

