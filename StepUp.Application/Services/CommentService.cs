using AutoMapper;
using StepUp.Application.DTOs.Comment;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CommentDto> CreateCommentAsync(CreateCommentDto createCommentDto, CancellationToken cancellationToken = default)
    {
        // Verify post exists
        var post = await _postRepository.GetByIdAsync(createCommentDto.PostId, cancellationToken);
        if (post == null)
            throw new InvalidOperationException("Post not found.");

        var comment = _mapper.Map<Comment>(createCommentDto);
        await _commentRepository.AddAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        // Reload with user
        var comments = await _commentRepository.GetByPostIdWithUserAsync(createCommentDto.PostId, cancellationToken);
        var commentWithUser = comments.FirstOrDefault(c => c.Id == comment.Id);
        return MapToCommentDto(commentWithUser!);
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByPostIdAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.GetByPostIdWithUserAsync(postId, cancellationToken);
        return comments.Select(MapToCommentDto);
    }

    public async Task<CommentDto> UpdateCommentAsync(Guid id, UpdateCommentDto updateCommentDto, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment == null)
            throw new InvalidOperationException("Comment not found.");

        if (!isAdmin && comment.UserId != userId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        comment.Content = updateCommentDto.Content;
        await _commentRepository.UpdateAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);

        // Reload with user
        var comments = await _commentRepository.GetByPostIdWithUserAsync(comment.PostId, cancellationToken);
        var updatedComment = comments.FirstOrDefault(c => c.Id == id);
        if (updatedComment == null)
            throw new InvalidOperationException("Comment not found after update.");
        
        return MapToCommentDto(updatedComment);
    }

    public async Task DeleteCommentAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var comment = await _commentRepository.GetByIdAsync(id, cancellationToken);
        if (comment == null)
            throw new InvalidOperationException("Comment not found.");

        if (!isAdmin && comment.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        await _commentRepository.DeleteAsync(comment, cancellationToken);
        await _commentRepository.SaveChangesAsync(cancellationToken);
    }

    private CommentDto MapToCommentDto(Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            UserName = comment.User?.Name ?? "Unknown",
            UserRole = (int)(comment.User?.Role ?? Role.User),
            UserProfileImageUrl = comment.User?.ProfileImageUrl,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}

