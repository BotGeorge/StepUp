using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.ActivityLog;
using StepUp.Application.DTOs.Health;
using StepUp.Application.Interfaces;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : BaseController
{
    private readonly IActivityService _activityService;
    private readonly IChallengeService _challengeService;
    private readonly IParticipationRepository _participationRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public HealthController(
        IActivityService activityService,
        IChallengeService challengeService,
        IParticipationRepository participationRepository,
        IActivityLogRepository activityLogRepository)
    {
        _activityService = activityService;
        _challengeService = challengeService;
        _participationRepository = participationRepository;
        _activityLogRepository = activityLogRepository;
    }

    [HttpPost("sync")]
    public async Task<ActionResult> SyncHealthData([FromBody] SyncHealthDataDto syncData, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (syncData == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datele de sincronizare lipsesc."
                });
            }

            if (syncData.UserId == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID utilizator invalid."
                });
            }

            // Parse date - handle both DateTime and string formats
            DateTime syncDate;
            try
            {
                if (syncData.Date == default(DateTime))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Data de sincronizare lipsă."
                    });
                }

                // Ensure the date is UTC (PostgreSQL requires UTC)
                syncDate = syncData.Date.Kind == DateTimeKind.Utc 
                    ? syncData.Date.Date 
                    : DateTime.SpecifyKind(syncData.Date.Date, DateTimeKind.Utc);
            }
            catch (Exception dateEx)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Format de dată invalid.",
                    error = dateEx.Message
                });
            }
            var today = DateTime.UtcNow.Date;

            // Validate date (only allow today or past dates, max 7 days old)
            if (syncDate > today)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Nu poți sincroniza date pentru viitor."
                });
            }

            if (syncDate < today.AddDays(-7))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Nu poți sincroniza date mai vechi de 7 zile."
                });
            }

            var logsCreated = new List<ActivityLogDto>();
            var errors = new List<string>();

            // Sync steps (if provided)
            if (syncData.Steps.HasValue && syncData.Steps.Value > 0)
            {
                try
                {
                    var stepsLog = new CreateActivityLogDto
                    {
                        UserId = syncData.UserId,
                        Date = syncDate, // Already UTC
                        MetricValue = syncData.Steps.Value,
                        MetricType = Domain.Enums.MetricType.Steps
                    };
                    var created = await _activityService.AddActivityLogAsync(stepsLog, cancellationToken);
                    logsCreated.Add(created);
                }
                catch (Exception ex)
                {
                    errors.Add($"Eroare la salvarea pașilor: {ex.Message}");
                    Console.WriteLine($"Error saving steps: {ex}");
                }
            }

            // Sync calories (if provided)
            if (syncData.Calories.HasValue && syncData.Calories.Value > 0)
            {
                try
                {
                    var caloriesLog = new CreateActivityLogDto
                    {
                        UserId = syncData.UserId,
                        Date = syncDate, // Already UTC
                        MetricValue = syncData.Calories.Value,
                        MetricType = Domain.Enums.MetricType.CalorieBurn
                    };
                    var created = await _activityService.AddActivityLogAsync(caloriesLog, cancellationToken);
                    logsCreated.Add(created);
                }
                catch (Exception ex)
                {
                    errors.Add($"Eroare la salvarea caloriilor: {ex.Message}");
                    Console.WriteLine($"Error saving calories: {ex}");
                }
            }

            // Recalculate scores for all active challenges the user participates in
            var challengesUpdated = new List<Guid>();
            if (logsCreated.Count > 0)
            {
                try
                {
                    // Get all participations for this user
                    var participations = await _participationRepository.GetByUserIdAsync(syncData.UserId, cancellationToken);
                    
                    // Get all active challenges
                    var activeChallenges = await _challengeService.GetActiveChallengesAsync(cancellationToken);
                    var activeChallengeIds = activeChallenges.Select(c => c.Id).ToList();
                    
                    // Filter participations to only active challenges
                    var activeParticipations = participations
                        .Where(p => activeChallengeIds.Contains(p.ChallengeId))
                        .ToList();
                    
                    // Recalculate scores for each active challenge
                    foreach (var participation in activeParticipations)
                    {
                        try
                        {
                            await _challengeService.CalculateAndUpdateScoresAsync(participation.ChallengeId, cancellationToken);
                            challengesUpdated.Add(participation.ChallengeId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error recalculating scores for challenge {participation.ChallengeId}: {ex.Message}");
                            // Don't fail the whole sync if score recalculation fails
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error recalculating challenge scores: {ex.Message}");
                    // Don't fail the whole sync if score recalculation fails
                }
            }

            return Ok(new
            {
                success = true,
                message = errors.Count > 0 
                    ? $"Date sincronizate cu succes, dar au apărut {errors.Count} erori." 
                    : "Date sincronizate cu succes.",
                data = new
                {
                    date = syncDate.ToString("yyyy-MM-dd"),
                    steps = syncData.Steps,
                    calories = syncData.Calories,
                    distance = syncData.Distance,
                    heartRate = syncData.HeartRate,
                    activeMinutes = syncData.ActiveMinutes,
                    source = syncData.Source,
                    logsCreated = logsCreated.Count,
                    challengesUpdated = challengesUpdated.Count,
                    syncedAt = DateTime.UtcNow
                },
                warnings = errors.Count > 0 ? errors : null
            });
        }
        catch (Exception ex)
        {
            // Log full exception for debugging
            Console.WriteLine($"Error in SyncHealthData: {ex}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la sincronizarea datelor.",
                error = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    [HttpGet("stats/{userId}")]
    public async Task<ActionResult> GetHealthStats(Guid userId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
    {
        try
        {
            DateTime ToUtcDateOnly(DateTime dt)
            {
                var dateOnly = dt.Date;
                return dt.Kind == DateTimeKind.Utc
                    ? dateOnly
                    : DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
            }

            var start = ToUtcDateOnly(startDate ?? DateTime.UtcNow);
            var end = ToUtcDateOnly(endDate ?? DateTime.UtcNow);

            // Get total steps and calories for the date range from ActivityLogs
            var totalSteps = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
                userId, start, end, Domain.Enums.MetricType.Steps, cancellationToken);
            
            var totalCalories = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
                userId, start, end, Domain.Enums.MetricType.CalorieBurn, cancellationToken);

            // Calculate days in range
            var daysInRange = (end - start).Days + 1;
            var averageSteps = daysInRange > 0 ? totalSteps / daysInRange : 0;
            var averageCalories = daysInRange > 0 ? totalCalories / daysInRange : 0;

            return Ok(new
            {
                success = true,
                data = new
                {
                    userId,
                    startDate = start,
                    endDate = end,
                    totalSteps = (int)totalSteps,
                    totalCalories = (int)totalCalories,
                    averageSteps = (int)averageSteps,
                    averageCalories = (int)averageCalories,
                    todayStats = new
                    {
                        steps = (start == DateTime.UtcNow.Date && end == DateTime.UtcNow.Date) ? (int)totalSteps : 0,
                        calories = (start == DateTime.UtcNow.Date && end == DateTime.UtcNow.Date) ? (int)totalCalories : 0,
                    }
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la obținerea statisticilor.",
                error = ex.Message
            });
        }
    }
}
