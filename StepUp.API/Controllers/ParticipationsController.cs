using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.Participation;
using StepUp.Application.Interfaces;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParticipationsController : BaseController
{
    private readonly IParticipationService _participationService;

    public ParticipationsController(IParticipationService participationService)
    {
        _participationService = participationService;
    }

    [HttpPost("join")]
    public async Task<ActionResult<ParticipationDto>> JoinChallenge([FromBody] JoinChallengeDto joinChallengeDto, CancellationToken cancellationToken)
    {
        try
        {
            var participation = await _participationService.JoinChallengeAsync(joinChallengeDto, cancellationToken);
            return CreatedAtAction(nameof(JoinChallenge), new { id = participation.Id }, participation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ParticipationDto>>> GetUserParticipations(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var participations = await _participationService.GetUserParticipationsAsync(userId, cancellationToken);
            return Ok(participations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching participations.", error = ex.Message });
        }
    }
}

