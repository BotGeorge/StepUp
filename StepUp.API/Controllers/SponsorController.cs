using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.Challenge;
using StepUp.Application.Interfaces;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/sponsor")]
public class SponsorController : BaseController
{
    private readonly IChallengeService _challengeService;

    public SponsorController(IChallengeService challengeService)
    {
        _challengeService = challengeService;
    }

    [HttpPost("challenges")]
    public async Task<ActionResult<ChallengeDto>> CreateSponsoredChallenge(
        [FromBody] CreateSponsoredChallengeDto createSponsoredChallengeDto,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validare: userId este obligatoriu
            if (userId == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "userId este obligatoriu pentru crearea challenge-urilor sponsorizate."
                });
            }

            // Validare: Name este obligatoriu
            if (string.IsNullOrWhiteSpace(createSponsoredChallengeDto.Name))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Name este obligatoriu."
                });
            }

            // Validare: StartDate trebuie să fie înainte de EndDate
            if (createSponsoredChallengeDto.StartDate >= createSponsoredChallengeDto.EndDate)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "StartDate trebuie să fie înainte de EndDate."
                });
            }

            // Validare: Prize este obligatoriu
            if (string.IsNullOrWhiteSpace(createSponsoredChallengeDto.Prize))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Prize este obligatoriu pentru challenge-urile sponsorizate."
                });
            }

            // Convertim CreateSponsoredChallengeDto la CreateChallengeDto
            var challengeMode = createSponsoredChallengeDto.ChallengeMode?.ToLower().Trim();
            var isEndless = challengeMode != "target";
            
            var createChallengeDto = new CreateChallengeDto
            {
                Name = createSponsoredChallengeDto.Name,
                MetricType = createSponsoredChallengeDto.MetricType,
                StartDate = createSponsoredChallengeDto.StartDate,
                EndDate = createSponsoredChallengeDto.EndDate,
                Description = createSponsoredChallengeDto.Description,
                IsSponsored = true, // Setat automat la true
                Prize = createSponsoredChallengeDto.Prize, // Prize obligatoriu
                SponsorId = userId, // Setat automat din userId
                TargetValue = isEndless ? null : createSponsoredChallengeDto.TargetValue,
                ExerciseType = createSponsoredChallengeDto.ExerciseType
            };

            var challengeDto = await _challengeService.CreateChallengeAsync(createChallengeDto, userId, cancellationToken);
            
            return CreatedAtAction(
                nameof(ChallengesController.GetChallengeById),
                "Challenges",
                new { id = challengeDto.Id },
                challengeDto);
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
                message = "A apărut o eroare la crearea challenge-ului sponsorizat.",
                error = ex.Message
            });
        }
    }

}

