using Microsoft.EntityFrameworkCore;
using StepUp.Application.Interfaces;
using StepUp.Domain.Entities;
using StepUp.Domain.Enums;
using StepUp.Infrastructure.Data;

namespace StepUp.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ActivityLog> AddAsync(ActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        activityLog.CreatedAt = DateTime.UtcNow;
        
        // Ensure Date is UTC (PostgreSQL requires UTC)
        if (activityLog.Date.Kind != DateTimeKind.Utc)
        {
            activityLog.Date = DateTime.SpecifyKind(activityLog.Date, DateTimeKind.Utc);
        }
        
        await _context.ActivityLogs.AddAsync(activityLog, cancellationToken);
        return activityLog;
    }

    public async Task<ActivityLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ActivityLogs.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ActivityLog>> GetByUserAsync(Guid userId, DateTime? startDate, DateTime? endDate, MetricType? metricType, int limit, CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityLogs.Where(a => a.UserId == userId);

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Date <= endDate.Value);
        }

        if (metricType.HasValue)
        {
            query = query.Where(a => a.MetricType == metricType.Value);
        }

        return await query
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await GetTotalMetricValueByUserAndDateRangeAsync(userId, startDate, endDate, null, null, null, cancellationToken);
    }

    public async Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, Domain.Enums.MetricType? metricType, CancellationToken cancellationToken = default)
    {
        return await GetTotalMetricValueByUserAndDateRangeAsync(userId, startDate, endDate, metricType, null, null, cancellationToken);
    }

    public async Task<decimal> GetTotalMetricValueByUserAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate, Domain.Enums.MetricType? metricType, string? exerciseType, DateTime? createdAtFrom, CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityLogs
            .Where(a => a.UserId == userId 
                     && a.Date >= startDate 
                     && a.Date <= endDate);

        // Filter by MetricType if provided
        if (metricType.HasValue)
        {
            query = query.Where(a => a.MetricType == metricType.Value);
        }

        if (!string.IsNullOrWhiteSpace(exerciseType))
        {
            var normalized = exerciseType.Trim();
            query = query.Where(a => a.ExerciseType == normalized);
        }

        if (createdAtFrom.HasValue)
        {
            var createdAtUtc = createdAtFrom.Value.Kind == DateTimeKind.Utc
                ? createdAtFrom.Value
                : DateTime.SpecifyKind(createdAtFrom.Value, DateTimeKind.Utc);
            query = query.Where(a => a.CreatedAt >= createdAtUtc);
        }

        var total = await query.SumAsync(a => a.MetricValue, cancellationToken);

        return total;
    }

    public Task UpdateAsync(ActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        activityLog.UpdatedAt = DateTime.UtcNow;
        _context.ActivityLogs.Update(activityLog);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ActivityLog activityLog, CancellationToken cancellationToken = default)
    {
        _context.ActivityLogs.Remove(activityLog);
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

