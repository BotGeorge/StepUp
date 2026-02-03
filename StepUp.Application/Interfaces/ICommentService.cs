using StepUp.Application.DTOs.Comment;

namespace StepUp.Application.Interfaces;

public interface ICommentService
{
    Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(Guid postId, CancellationToken cancellationToken = default);
    Task<CommentDto> UpdateCommentAsync(Guid id, UpdateCommentDto updateCommentDto, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task DeleteCommentAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default);
}

