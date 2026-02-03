using StepUp.Application.DTOs.ActivityLog;
using StepUp.Domain.Enums;

namespace StepUp.Application.Interfaces;

public interface IActivityService
{
    Task<ActivityLogDto> AddActivityLogAsync(CreateActivityLogDto createActivityLogDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityLogDto>> GetActivityLogsByUserAsync(Guid userId, DateTime? startDate, DateTime? endDate, MetricType? metricType, int limit, CancellationToken cancellationToken = default);
    Task<ActivityLogDto?> UpdateActivityLogAsync(Guid id, UpdateActivityLogDto updateActivityLogDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteActivityLogAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ActivitySummaryDto> GetActivitySummaryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityDailySummaryDto>> GetDailySummaryAsync(Guid userId, int days, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, MetricType? metricType, CancellationToken cancellationToken = default);
}

