using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.Challenge;
using StepUp.Application.DTOs.Leaderboard;
using StepUp.Application.DTOs.Participation;
using StepUp.Application.Interfaces;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengesController : BaseController
{
    private readonly IChallengeService _challengeService;
    private readonly IParticipationService _participationService;

    public ChallengesController(IChallengeService challengeService, IParticipationService participationService)
    {
        _challengeService = challengeService;
        _participationService = participationService;
    }

    [HttpPost]
    public async Task<ActionResult<ChallengeDto>> CreateChallenge([FromBody] CreateChallengeDto createChallengeDto, [FromQuery] Guid? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var challengeDto = await _challengeService.CreateChallengeAsync(createChallengeDto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetAllChallenges), new { }, challengeDto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la crearea challenge-ului.",
                error = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChallengeDto>>> GetAllChallenges([FromQuery] Guid? userId, CancellationToken cancellationToken = default)
    {
        IEnumerable<ChallengeDto> challenges;
        
        // Dacă userId este furnizat, returnează doar challenge-urile vizibile pentru utilizator
        if (userId.HasValue)
        {
            challenges = await _challengeService.GetVisibleChallengesForUserAsync(userId.Value, cancellationToken);
        }
        else
        {
            // Dacă nu este furnizat userId, returnează toate challenge-urile (comportament vechi pentru compatibilitate)
            challenges = await _challengeService.GetAllChallengesAsync(cancellationToken);
        }
        
        return Ok(challenges);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ChallengeDto>>> GetActiveChallenges(CancellationToken cancellationToken)
    {
        var challenges = await _challengeService.GetActiveChallengesAsync(cancellationToken);
        return Ok(challenges);
    }

    [HttpGet("with-stats")]
    public async Task<ActionResult<IEnumerable<ChallengeWithStatsDto>>> GetAllChallengesWithStats(CancellationToken cancellationToken)
    {
        var challenges = await _challengeService.GetAllChallengesWithStatsAsync(cancellationToken);
        return Ok(challenges);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ChallengeDto>> GetChallengeById(Guid id, [FromQuery] Guid? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var challenge = await _challengeService.GetChallengeByIdAsync(id, userId, cancellationToken);
            if (challenge == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Challenge-ul nu a fost găsit."
                });
            }
            return Ok(challenge);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Nu logăm erorile de acces neautorizat ca erori fizice - sunt așteptate
            return StatusCode(403, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la obținerea challenge-ului.",
                error = ex.Message
            });
        }
    }

    [HttpPost("{challengeId}/join/{userId}")]
    public async Task<ActionResult> JoinChallenge(Guid challengeId, Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            var joinChallengeDto = new JoinChallengeDto
            {
                UserId = userId,
                ChallengeId = challengeId
            };

            var participation = await _participationService.JoinChallengeAsync(joinChallengeDto, cancellationToken);
            
            return Ok(new
            {
                success = true,
                message = "Utilizatorul s-a înscris cu succes la challenge.",
                participation = participation
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la înscrierea la challenge.",
                error = ex.Message
            });
        }
    }

    [HttpGet("{id}/leaderboard")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboard(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var leaderboard = await _challengeService.GetLeaderboardAsync(id, cancellationToken);
            return Ok(leaderboard);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la obținerea leaderboard-ului.",
                error = ex.Message
            });
        }
    }

    [HttpPost("mark-expired-as-completed")]
    public async Task<ActionResult> MarkExpiredChallengesAsCompleted(CancellationToken cancellationToken)
    {
        try
        {
            await _challengeService.MarkExpiredChallengesAsCompletedAsync(cancellationToken);
            return Ok(new
            {
                success = true,
                message = "Challenge-urile expirate au fost marcate ca finalizate."
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la marcarea challenge-urilor expirate.",
                error = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteChallenge(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "ID utilizator invalid." });
            }

            var deleted = await _challengeService.DeleteChallengeAsync(id, userId, cancellationToken);
            if (!deleted)
            {
                return NotFound(new { success = false, message = "Challenge-ul nu a fost găsit." });
            }

            return Ok(new { success = true, message = "Challenge-ul a fost șters cu succes." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "A apărut o eroare la ștergerea challenge-ului.", error = ex.Message });
        }
    }
}

