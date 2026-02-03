using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.Friend;
using StepUp.Application.Interfaces;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/friends")]
public class FriendsController : BaseController
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    [HttpPost("request")]
    public async Task<ActionResult<FriendRequestDto>> SendFriendRequest(
        [FromBody] SendFriendRequestByNameDto sendFriendRequestDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validare: numele este obligatoriu
            if (string.IsNullOrWhiteSpace(sendFriendRequestDto.ToUserName))
            {
                return BadRequest(new { success = false, message = "Numele utilizatorului este obligatoriu." });
            }

            var request = await _friendService.SendFriendRequestByNameAsync(
                sendFriendRequestDto.FromUserId,
                sendFriendRequestDto.ToUserName,
                cancellationToken);
            return Ok(new { success = true, message = "Cererea de prietenie a fost trimisă.", data = request });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpPost("request/{id}/accept")]
    public async Task<ActionResult<FriendRequestDto>> AcceptFriendRequest(
        Guid id,
        [FromBody] AcceptFriendRequestDto acceptDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _friendService.AcceptFriendRequestAsync(id, acceptDto.UserId, cancellationToken);
            return Ok(new { success = true, message = "Cererea de prietenie a fost acceptată.", data = request });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpPost("request/{id}/decline")]
    public async Task<ActionResult> DeclineFriendRequest(
        Guid id,
        [FromBody] DeclineFriendRequestDto declineDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _friendService.DeclineFriendRequestAsync(id, declineDto.UserId, cancellationToken);
            return Ok(new { success = true, message = "Cererea de prietenie a fost refuzată." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpDelete("request/{requestId}")]
    public async Task<ActionResult> CancelFriendRequest(
        Guid requestId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _friendService.CancelFriendRequestAsync(requestId, userId, cancellationToken);
            return Ok(new { success = true, message = "Cererea de prietenie a fost anulată." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetPendingRequests(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "ID utilizator invalid." });
            }

            var requests = await _friendService.GetPendingRequestsAsync(userId, cancellationToken);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpGet("sent")]
    public async Task<ActionResult<IEnumerable<FriendRequestDto>>> GetSentRequests(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requests = await _friendService.GetSentRequestsAsync(userId, cancellationToken);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetFriends(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var friends = await _friendService.GetFriendsAsync(userId, cancellationToken);
            return Ok(friends);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpGet("profile/{friendId}")]
    public async Task<ActionResult<FriendProfileDto>> GetFriendProfile(
        Guid friendId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _friendService.GetFriendProfileAsync(userId, friendId, cancellationToken);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<FriendDto>>> GetFriendsByUserId(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var friends = await _friendService.GetFriendsAsync(userId, cancellationToken);
            return Ok(friends);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpGet("check")]
    public async Task<ActionResult<bool>> AreFriends(
        [FromQuery] Guid userId1,
        [FromQuery] Guid userId2,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var areFriends = await _friendService.AreFriendsAsync(userId1, userId2, cancellationToken);
            return Ok(new { areFriends });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }

    [HttpDelete("{userId}/{friendId}")]
    public async Task<ActionResult> RemoveFriend(
        Guid userId,
        Guid friendId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _friendService.RemoveFriendAsync(userId, friendId, cancellationToken);
            return Ok(new { success = true, message = "Prietenia a fost ștearsă." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare.", error = ex.Message });
        }
    }
}

