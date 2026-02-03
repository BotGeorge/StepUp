using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.Post;
using StepUp.Application.Interfaces;
using StepUp.Domain.Enums;
using StepUp.Infrastructure.Repositories;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : BaseController
{
    private readonly IPostService _postService;
    private readonly IUserRepository _userRepository;

    public PostsController(IPostService postService, IUserRepository userRepository)
    {
        _postService = postService;
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost([FromBody] CreatePostDto createPostDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var post = await _postService.CreatePostAsync(createPostDto, cancellationToken);
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare la crearea postării.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetAllPosts([FromQuery] Guid? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<PostDto> posts;
            
            // Dacă userId este furnizat
            if (userId.HasValue)
            {
                // Verifică dacă utilizatorul este admin
                var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
                if (user != null && user.Role == Role.Admin)
                {
                    // Adminii văd toate postările
                    posts = await _postService.GetAllPostsAsync(cancellationToken);
                }
                else
                {
                    // Utilizatorii normali văd postările de la prieteni + Partner/Admin
                    posts = await _postService.GetPostsByFriendsAsync(userId.Value, cancellationToken);
                }
            }
            else
            {
                // Dacă nu este furnizat userId, returnează toate postările (comportament vechi pentru compatibilitate)
                posts = await _postService.GetAllPostsAsync(cancellationToken);
            }
            
            return Ok(posts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare la obținerea postărilor.", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostDto>> GetPostById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(id, cancellationToken);
            if (post == null)
                return NotFound(new { success = false, message = "Postarea nu a fost găsită." });

            return Ok(post);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PostDto>> UpdatePost(
        Guid id,
        [FromBody] UpdatePostDto updatePostDto,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                return Unauthorized(new { success = false, message = "Utilizator invalid." });

            bool isAdmin = user.Role == Role.Admin;
            var post = await _postService.UpdatePostAsync(id, updatePostDto, userId, isAdmin, cancellationToken);
            return Ok(new { success = true, message = "Postarea a fost actualizată.", data = post });
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
    public async Task<ActionResult> DeletePost(
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
            await _postService.DeletePostAsync(id, userId, isAdmin, cancellationToken);
            return Ok(new { success = true, message = "Postarea a fost ștearsă." });
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

