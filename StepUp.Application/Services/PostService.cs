using AutoMapper;
using StepUp.Application.DTOs.Post;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IMapper _mapper;

    public PostService(IPostRepository postRepository, IUserRepository userRepository, IFriendRequestRepository friendRequestRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _friendRequestRepository = friendRequestRepository;
        _mapper = mapper;
    }

    public async Task<PostDto> CreatePostAsync(CreatePostDto createPostDto, CancellationToken cancellationToken = default)
    {
        var post = _mapper.Map<Post>(createPostDto);
        await _postRepository.AddAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        var postWithUser = await _postRepository.GetByIdWithUserAndCommentsAsync(post.Id, cancellationToken);
        return MapToPostDto(postWithUser!);
    }

    public async Task<IEnumerable<PostDto>> GetAllPostsAsync(CancellationToken cancellationToken = default)
    {
        var posts = await _postRepository.GetAllWithUserAsync(cancellationToken);
        return posts.Select(MapToPostDto);
    }

    public async Task<IEnumerable<PostDto>> GetPostsByFriendsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Obține lista de prieteni
        var friendRequests = await _friendRequestRepository.GetFriendsAsync(userId, cancellationToken);
        var friendIds = new List<Guid>();
        
        foreach (var request in friendRequests)
        {
            // Determină care user este prietenul (cel care nu este userId)
            var friendId = request.FromUserId == userId ? request.ToUserId : request.FromUserId;
            friendIds.Add(friendId);
        }

        // Adaugă și propriul ID pentru a include postările proprii
        friendIds.Add(userId);

        // Elimină duplicate-urile
        friendIds = friendIds.Distinct().ToList();

        // Roluri pentru care toți utilizatorii pot vedea postările (Partner = 2, Admin = 1)
        var specialRoles = new List<int> { (int)Role.Partner, (int)Role.Admin };

        // Obține postările de la prieteni + postările proprii + postările de la Partner/Admin
        var posts = await _postRepository.GetByFriendIdsAndRolesAsync(friendIds, specialRoles, cancellationToken);
        return posts.Select(MapToPostDto);
    }

    public async Task<PostDto?> GetPostByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdWithUserAndCommentsAsync(id, cancellationToken);
        if (post == null)
            return null;

        return MapToPostDto(post);
    }

    public async Task<PostDto> UpdatePostAsync(Guid id, UpdatePostDto updatePostDto, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        if (post == null)
            throw new InvalidOperationException("Post not found.");

        if (!isAdmin && post.UserId != userId)
            throw new UnauthorizedAccessException("You can only edit your own posts.");

        post.Content = updatePostDto.Content;
        post.ImageUrl = updatePostDto.ImageUrl;
        await _postRepository.UpdateAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);

        var updatedPost = await _postRepository.GetByIdWithUserAndCommentsAsync(id, cancellationToken);
        return MapToPostDto(updatedPost!);
    }

    public async Task DeletePostAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var post = await _postRepository.GetByIdAsync(id, cancellationToken);
        if (post == null)
            throw new InvalidOperationException("Post not found.");

        if (!isAdmin && post.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own posts.");

        await _postRepository.DeleteAsync(post, cancellationToken);
        await _postRepository.SaveChangesAsync(cancellationToken);
    }

    private PostDto MapToPostDto(Post post)
    {
        return new PostDto
        {
            Id = post.Id,
            UserId = post.UserId,
            UserName = post.User?.Name ?? "Unknown",
            UserRole = (int)(post.User?.Role ?? Role.User),
            UserProfileImageUrl = post.User?.ProfileImageUrl,
            Content = post.Content,
            ImageUrl = post.ImageUrl,
            CommentCount = post.Comments?.Count ?? 0,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }
}

