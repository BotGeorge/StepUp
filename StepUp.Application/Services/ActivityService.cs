using AutoMapper;
using StepUp.Application.DTOs.ActivityLog;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IMapper _mapper;

    public ActivityService(IActivityLogRepository activityLogRepository, IMapper mapper)
    {
        _activityLogRepository = activityLogRepository;
        _mapper = mapper;
    }

    public async Task<ActivityLogDto> AddActivityLogAsync(CreateActivityLogDto createActivityLogDto, CancellationToken cancellationToken = default)
    {
        var activityLog = _mapper.Map<ActivityLog>(createActivityLogDto);
        
        await _activityLogRepository.AddAsync(activityLog, cancellationToken);
        await _activityLogRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ActivityLogDto>(activityLog);
    }

    public async Task<IEnumerable<ActivityLogDto>> GetActivityLogsByUserAsync(Guid userId, DateTime? startDate, DateTime? endDate, MetricType? metricType, int limit, CancellationToken cancellationToken = default)
    {
        var logs = await _activityLogRepository.GetByUserAsync(userId, startDate, endDate, metricType, limit, cancellationToken);
        return logs.Select(log => _mapper.Map<ActivityLogDto>(log));
    }

    public async Task<ActivityLogDto?> UpdateActivityLogAsync(Guid id, UpdateActivityLogDto updateActivityLogDto, CancellationToken cancellationToken = default)
    {
        var activityLog = await _activityLogRepository.GetByIdAsync(id, cancellationToken);
        if (activityLog == null)
        {
            return null;
        }

        activityLog.MetricValue = updateActivityLogDto.MetricValue;
        activityLog.Date = updateActivityLogDto.Date;
        activityLog.MetricType = updateActivityLogDto.MetricType;
        activityLog.ExerciseType = updateActivityLogDto.ExerciseType;

        await _activityLogRepository.UpdateAsync(activityLog, cancellationToken);
        await _activityLogRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ActivityLogDto>(activityLog);
    }

    public async Task<bool> DeleteActivityLogAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activityLog = await _activityLogRepository.GetByIdAsync(id, cancellationToken);
        if (activityLog == null)
        {
            return false;
        }

        await _activityLogRepository.DeleteAsync(activityLog, cancellationToken);
        await _activityLogRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ActivitySummaryDto> GetActivitySummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var summary = new ActivitySummaryDto
        {
            UserId = userId,
        };

        // Load last 365 days of logs for streaks/achievements
        var start = DateTime.UtcNow.Date.AddDays(-365);
        var end = DateTime.UtcNow.Date;
        var logs = await _activityLogRepository.GetByUserAsync(userId, start, end, null, 10000, cancellationToken);
        var logList = logs.ToList();

        summary.TotalActivities = logList.Count;
        summary.TotalSteps = (int)logList.Where(l => l.MetricType == MetricType.Steps).Sum(l => l.MetricValue);
        summary.TotalCalories = (int)logList.Where(l => l.MetricType == MetricType.CalorieBurn).Sum(l => l.MetricValue);

        // Distinct activity dates
        var activeDates = logList
            .Select(l => l.Date.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // Current streak (ending today)
        var currentStreak = 0;
        var dayCursor = DateTime.UtcNow.Date;
        var activeDateSet = activeDates.ToHashSet();
        while (activeDateSet.Contains(dayCursor))
        {
            currentStreak++;
            dayCursor = dayCursor.AddDays(-1);
        }
        summary.CurrentStreakDays = currentStreak;

        // Best streak
        var bestStreak = 0;
        var runningStreak = 0;
        DateTime? prev = null;
        foreach (var d in activeDates)
        {
            if (prev.HasValue && d == prev.Value.AddDays(1))
            {
                runningStreak++;
            }
            else
            {
                runningStreak = 1;
            }
            if (runningStreak > bestStreak)
            {
                bestStreak = runningStreak;
            }
            prev = d;
        }
        summary.BestStreakDays = bestStreak;

        // Achievements
        var achievements = new List<string>();
        if (summary.TotalActivities > 0)
        {
            achievements.Add("first_activity");
        }
        if (bestStreak >= 7)
        {
            achievements.Add("streak_7");
        }
        if (bestStreak >= 30)
        {
            achievements.Add("streak_30");
        }

        // 10k steps in a day
        var stepsByDay = logList
            .Where(l => l.MetricType == MetricType.Steps)
            .GroupBy(l => l.Date.Date)
            .Select(g => g.Sum(x => x.MetricValue))
            .ToList();
        if (stepsByDay.Any(total => total >= 10000))
        {
            achievements.Add("steps_10k");
        }

        summary.Achievements = achievements;
        return summary;
    }

    public async Task<IEnumerable<ActivityDailySummaryDto>> GetDailySummaryAsync(Guid userId, int days, CancellationToken cancellationToken = default)
    {
        var safeDays = Math.Clamp(days, 1, 90);
        var end = DateTime.UtcNow.Date;
        var start = end.AddDays(-safeDays + 1);

        var logs = await _activityLogRepository.GetByUserAsync(userId, start, end, null, 10000, cancellationToken);
        var logList = logs.ToList();

        var grouped = logList
            .GroupBy(l => l.Date.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<ActivityDailySummaryDto>();
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            grouped.TryGetValue(d, out var dayLogs);
            dayLogs ??= new List<ActivityLog>();

            var summary = new ActivityDailySummaryDto
            {
                Date = d,
                Steps = (int)dayLogs.Where(l => l.MetricType == MetricType.Steps).Sum(l => l.MetricValue),
                Calories = (int)dayLogs.Where(l => l.MetricType == MetricType.CalorieBurn).Sum(l => l.MetricValue),
                Running = dayLogs.Where(l => l.MetricType == MetricType.Running).Sum(l => l.MetricValue),
                PhysicalExercises = dayLogs.Where(l => l.MetricType == MetricType.PhysicalExercises).Sum(l => l.MetricValue),
                TotalActivities = dayLogs.Count
            };

            results.Add(summary);
        }

        return results;
    }

    public async Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, MetricType? metricType, CancellationToken cancellationToken = default)
    {
        return await _activityLogRepository.GetTotalMetricValueByUserAndDateRangeAsync(userId, startDate, endDate, metricType, cancellationToken);
    }
}

