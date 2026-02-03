using Microsoft.AspNetCore.Mvc;
using StepUp.Application.DTOs.ActivityLog;
using StepUp.Application.Interfaces;
using StepUp.Domain.Enums;

namespace StepUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityController : BaseController
{
    private readonly IActivityService _activityService;
    private readonly IChallengeService _challengeService;
    private readonly IParticipationRepository _participationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityController(
        IActivityService activityService,
        IChallengeService challengeService,
        IParticipationRepository participationRepository,
        IUserRepository userRepository,
        IActivityLogRepository activityLogRepository)
    {
        _activityService = activityService;
        _challengeService = challengeService;
        _participationRepository = participationRepository;
        _userRepository = userRepository;
        _activityLogRepository = activityLogRepository;
    }

    [HttpPost]
    public async Task<ActionResult<ActivityLogDto>> AddActivity([FromBody] CreateActivityLogDto createActivityLogDto, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (createActivityLogDto == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Datele activității lipsesc."
                });
            }

            if (createActivityLogDto.UserId == Guid.Empty)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ID utilizator invalid."
                });
            }

            // Ensure date is UTC date-only (PostgreSQL requires UTC)
            var activityDate = ToUtcDateOnly(createActivityLogDto.Date);

            var today = DateTime.UtcNow.Date;

            // Validate date (only allow today or past dates, max 7 days old)
            if (activityDate > today)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Nu poți adăuga activități pentru viitor."
                });
            }

            if (activityDate < today.AddDays(-7))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Nu poți adăuga activități mai vechi de 7 zile."
                });
            }

            // Set the date to UTC
            createActivityLogDto.Date = activityDate;

            if (createActivityLogDto.MetricType == Domain.Enums.MetricType.PhysicalExercises)
            {
                if (string.IsNullOrWhiteSpace(createActivityLogDto.ExerciseType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Trebuie să selectezi tipul de exerciții fizice."
                    });
                }
                createActivityLogDto.ExerciseType = createActivityLogDto.ExerciseType.Trim();
            }
            else
            {
                createActivityLogDto.ExerciseType = null;
            }

            if (createActivityLogDto.MetricType == Domain.Enums.MetricType.CalorieBurn
                && createActivityLogDto.ParentActivityLogId == null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Caloriile arse se generează automat după o activitate."
                });
            }

            // Add activity log
            var activityLog = await _activityService.AddActivityLogAsync(createActivityLogDto, cancellationToken);

            decimal? estimatedCalories = null;
            decimal? estimatedDistanceKm = null;
            if (createActivityLogDto.MetricType != Domain.Enums.MetricType.CalorieBurn)
            {
                var user = await _userRepository.GetByIdAsync(createActivityLogDto.UserId, cancellationToken);
                if (user != null)
                {
                    (estimatedCalories, estimatedDistanceKm) = EstimateCalories(
                        user,
                        createActivityLogDto.MetricType,
                        createActivityLogDto.MetricValue,
                        createActivityLogDto.ExerciseType);
                    if (estimatedCalories.HasValue && estimatedCalories.Value > 0)
                    {
                        // Add calculated calories as an additional activity log
                        var caloriesLog = new CreateActivityLogDto
                        {
                            UserId = createActivityLogDto.UserId,
                            Date = createActivityLogDto.Date,
                            MetricValue = estimatedCalories.Value,
                            MetricType = Domain.Enums.MetricType.CalorieBurn,
                            ExerciseType = null,
                            ParentActivityLogId = activityLog.Id
                        };
                        await _activityService.AddActivityLogAsync(caloriesLog, cancellationToken);
                    }
                }
            }

            var challengesUpdated = await RecalculateActiveChallengesForUser(createActivityLogDto.UserId, cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Activitatea a fost adăugată cu succes.",
                data = activityLog,
                challengesUpdated = challengesUpdated.Count,
                estimatedCalories,
                estimatedDistanceKm
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in AddActivity: {ex}");
            return StatusCode(500, new
            {
                success = false,
                message = "A apărut o eroare la adăugarea activității.",
                error = ex.Message
            });
        }
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ActivityLogDto>>> GetActivitiesByUser(
        Guid userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? metricType,
        [FromQuery] int limit = 30,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new
            {
                success = false,
                message = "ID utilizator invalid."
            });
        }

        var start = startDate.HasValue ? ToUtcDateOnly(startDate.Value) : (DateTime?)null;
        var end = endDate.HasValue ? ToUtcDateOnly(endDate.Value) : (DateTime?)null;

        Domain.Enums.MetricType? metric = null;
        if (metricType.HasValue && Enum.IsDefined(typeof(Domain.Enums.MetricType), metricType.Value))
        {
            metric = (Domain.Enums.MetricType)metricType.Value;
        }

        var logs = await _activityService.GetActivityLogsByUserAsync(
            userId,
            start,
            end,
            metric,
            Math.Clamp(limit, 1, 200),
            cancellationToken);
        return Ok(logs);
    }

    [HttpGet("summary/{userId}")]
    public async Task<ActionResult<ActivitySummaryDto>> GetActivitySummary(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new
            {
                success = false,
                message = "ID utilizator invalid."
            });
        }

        var summary = await _activityService.GetActivitySummaryAsync(userId, cancellationToken);
        return Ok(summary);
    }

    [HttpGet("daily-summary/{userId}")]
    public async Task<ActionResult<IEnumerable<ActivityDailySummaryDto>>> GetDailySummary(
        Guid userId,
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new
            {
                success = false,
                message = "ID utilizator invalid."
            });
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Utilizatorul nu a fost găsit."
            });
        }

        var safeDays = Math.Clamp(days, 1, 90);
        var today = DateTime.UtcNow.Date;
        var createdDate = user.CreatedAt.Date;
        var availableDays = (today - createdDate).Days + 1;
        if (availableDays < 1)
        {
            availableDays = 1;
        }
        var effectiveDays = Math.Min(safeDays, availableDays);

        var summaries = await _activityService.GetDailySummaryAsync(userId, effectiveDays, cancellationToken);
        return Ok(summaries);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateActivity(Guid id, [FromBody] UpdateActivityLogDto updateActivityLogDto, CancellationToken cancellationToken)
    {
        if (updateActivityLogDto == null || updateActivityLogDto.UserId == Guid.Empty)
        {
            return BadRequest(new
            {
                success = false,
                message = "Datele activității sunt invalide."
            });
        }

        var activityDate = ToUtcDateOnly(updateActivityLogDto.Date);
        var today = DateTime.UtcNow.Date;
        if (activityDate > today)
        {
            return BadRequest(new { success = false, message = "Nu poți seta activități pentru viitor." });
        }
        if (activityDate < today.AddDays(-7))
        {
            return BadRequest(new { success = false, message = "Nu poți modifica activități mai vechi de 7 zile." });
        }

        updateActivityLogDto.Date = activityDate;

        if (updateActivityLogDto.MetricType == Domain.Enums.MetricType.PhysicalExercises)
        {
            if (string.IsNullOrWhiteSpace(updateActivityLogDto.ExerciseType))
            {
                return BadRequest(new { success = false, message = "Trebuie să selectezi tipul de exerciții fizice." });
            }
            updateActivityLogDto.ExerciseType = updateActivityLogDto.ExerciseType.Trim();
        }
        else
        {
            updateActivityLogDto.ExerciseType = null;
        }

        var updated = await _activityService.UpdateActivityLogAsync(id, updateActivityLogDto, cancellationToken);
        if (updated == null)
        {
            return NotFound(new { success = false, message = "Activitatea nu a fost găsită." });
        }

        var challengesUpdated = await RecalculateActiveChallengesForUser(updateActivityLogDto.UserId, cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Activitatea a fost actualizată cu succes.",
            data = updated,
            challengesUpdated = challengesUpdated.Count
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteActivity(Guid id, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "ID utilizator invalid." });
        }

        var deleted = await _activityService.DeleteActivityLogAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound(new { success = false, message = "Activitatea nu a fost găsită." });
        }

        var challengesUpdated = await RecalculateActiveChallengesForUser(userId, cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Activitatea a fost ștearsă cu succes.",
            challengesUpdated = challengesUpdated.Count
        });
    }

    private DateTime ToUtcDateOnly(DateTime dt)
    {
        var dateOnly = dt.Date;
        return dt.Kind == DateTimeKind.Utc
            ? dateOnly
            : DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
    }

    [HttpGet("daily-metrics/{userId}")]
    public async Task<ActionResult> GetDailyMetrics(Guid userId, [FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new { success = false, message = "ID utilizator invalid." });
        }

        var day = ToUtcDateOnly(date ?? DateTime.UtcNow);

        var steps = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
            userId, day, day, MetricType.Steps, null, null, cancellationToken);

        var calories = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
            userId, day, day, MetricType.CalorieBurn, null, null, cancellationToken);

        var runningKm = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
            userId, day, day, MetricType.Running, null, null, cancellationToken);

        var pushups = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
            userId, day, day, MetricType.PhysicalExercises, "Flotări", null, cancellationToken);

        var squats = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
            userId, day, day, MetricType.PhysicalExercises, "Genuflexiuni", null, cancellationToken);

        var abs = await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(
            userId, day, day, MetricType.PhysicalExercises, "Abdomene", null, cancellationToken);

        return Ok(new
        {
            steps,
            calories,
            runningKm,
            pushups,
            squats,
            abs
        });
    }

    private (decimal? calories, decimal? distanceKm) EstimateCalories(
        StepUp.Domain.Entities.User user,
        Domain.Enums.MetricType metricType,
        decimal metricValue,
        string? exerciseType = null)
    {
        if (!user.WeightKg.HasValue || user.WeightKg.Value <= 0)
        {
            return (null, null);
        }

        var weightKg = user.WeightKg.Value;

        if (metricType == Domain.Enums.MetricType.Steps)
        {
            if (!user.HeightCm.HasValue || user.HeightCm.Value <= 0)
            {
                return (null, null);
            }

            // Estimate distance based on stride length ~ 0.415 * height (cm)
            var strideCm = user.HeightCm.Value * 0.415m;
            var distanceKm = metricValue * strideCm / 100000m;
            if (distanceKm <= 0)
            {
                return (null, null);
            }

            // Walking calories estimate: ~0.53 kcal per kg per km
            var calories = 0.53m * weightKg * distanceKm;
            return (Math.Round(calories, 2), Math.Round(distanceKm, 3));
        }

        if (metricType == Domain.Enums.MetricType.Running)
        {
            // Running calories estimate: ~1.036 kcal per kg per km
            var distanceKm = metricValue;
            if (distanceKm <= 0)
            {
                return (null, null);
            }
            var calories = 1.036m * weightKg * distanceKm;
            return (Math.Round(calories, 2), Math.Round(distanceKm, 3));
        }

        if (metricType == Domain.Enums.MetricType.PhysicalExercises)
        {
            // Estimate calories based on MET and approximate duration from reps.
            var reps = metricValue;
            if (reps <= 0)
            {
                return (null, null);
            }

            // Assume ~30 reps/min as a rough pace.
            var minutes = reps / 30m;
            if (minutes <= 0)
            {
                return (null, null);
            }

            var normalized = (exerciseType ?? string.Empty).Trim().ToLowerInvariant();
            var met = normalized switch
            {
                "flotări" => 8.0m,
                "flotari" => 8.0m,
                "genuflexiuni" => 5.0m,
                "geno" => 5.0m,
                "abdomene" => 3.8m,
                _ => 6.0m
            };

            // Calories/min = MET * 3.5 * kg / 200
            var calories = met * 3.5m * weightKg / 200m * minutes;
            return (Math.Round(calories, 2), null);
        }

        return (null, null);
    }

    private async Task<List<Guid>> RecalculateActiveChallengesForUser(Guid userId, CancellationToken cancellationToken)
    {
        var challengesUpdated = new List<Guid>();
        try
        {
            var participations = await _participationRepository.GetByUserIdAsync(userId, cancellationToken);
            var activeChallenges = await _challengeService.GetActiveChallengesAsync(cancellationToken);
            var activeChallengeIds = activeChallenges.Select(c => c.Id).ToList();

            var activeParticipations = participations
                .Where(p => activeChallengeIds.Contains(p.ChallengeId))
                .ToList();

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
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error recalculating challenge scores: {ex.Message}");
        }

        return challengesUpdated;
    }
}

