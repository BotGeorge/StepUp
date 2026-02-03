using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.Comment;
using StepUp.Application.Interfaces;
using StepUp.Domain.Enums;
using StepUp.Infrastructure.Repositories;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : BaseController
{
    private readonly ICommentService _commentService;
    private readonly IUserRepository _userRepository;

    public CommentsController(ICommentService commentService, IUserRepository userRepository)
    {
        _commentService = commentService;
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto createCommentDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var comment = await _commentService.CreateCommentAsync(createCommentDto, cancellationToken);
            return CreatedAtAction(nameof(GetCommentsByPostId), new { postId = createCommentDto.PostId }, comment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare la crearea comentariului.", error = ex.Message });
        }
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByPostId(Guid postId, CancellationToken cancellationToken = default)
    {
        try
        {
            var comments = await _commentService.GetCommentsByPostIdAsync(postId, cancellationToken);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CommentDto>> UpdateComment(
        Guid id,
        [FromBody] UpdateCommentDto updateCommentDto,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Unauthorized(new { success = false, message = "Utilizator invalid." });

            bool isAdmin = user.Role == Role.Admin;
            var comment = await _commentService.UpdateCommentAsync(id, updateCommentDto, userId, isAdmin, cancellationToken);
            return Ok(new { success = true, message = "Comentariul a fost actualizat.", data = comment });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComment(
        Guid id,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Unauthorized(new { success = false, message = "Utilizator invalid." });

            bool isAdmin = user.Role == Role.Admin;
            await _commentService.DeleteCommentAsync(id, userId, isAdmin, cancellationToken);
            return Ok(new { success = true, message = "Comentariul a fost șters." });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }
}

