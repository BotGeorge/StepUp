using StepUp.Domain.Entities;
using StepUp.Domain.Enums;

namespace StepUp.Application.Interfaces;

public interface IActivityLogRepository
{
    Task<ActivityLog> AddAsync(ActivityLog activityLog, CancellationToken cancellationToken = default);
    Task<ActivityLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityLog>> GetByUserAsync(Guid userId, DateTime? startDate, DateTime? endDate, MetricType? metricType, int limit, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, MetricType? metricType, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, MetricType? metricType, string? exerciseType, DateTime? createdAtFrom, CancellationToken cancellationToken = default);
    Task UpdateAsync(ActivityLog activityLog, CancellationToken cancellationToken = default);
    Task DeleteAsync(ActivityLog activityLog, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

