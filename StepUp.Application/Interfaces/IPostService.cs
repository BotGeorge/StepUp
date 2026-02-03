using StepUp.Application.DTOs.Post;

namespace StepUp.Application.Interfaces;

public interface IPostService
{
    Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<PostDto>> GetAllPostsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PostDto>> GetPostsByFriendsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PostDto?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PostDto> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task DeletePostAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
}

