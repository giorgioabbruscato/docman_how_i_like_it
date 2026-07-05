using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.TimeTracking.Infrastructure.Analytics;

internal sealed class TimeEntryAnalyticsProvider : ITimeEntryAnalyticsProvider
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;
    private readonly ITimesheetRepository _timesheetRepository;

    public TimeEntryAnalyticsProvider(
        DbContext dbContext,
        ITenantContextAccessor accessor,
        ITimesheetRepository timesheetRepository)
    {
        _dbContext = dbContext;
        _accessor = accessor;
        _timesheetRepository = timesheetRepository;
    }

    public async Task<int> GetTotalMinutesAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildFilteredQueryAsync(
            from, to, projectId, employeeId, allowedEmployeeIds, completedOnly: true, cancellationToken);
        return await query.SumAsync(t => t.WorkedMinutes, cancellationToken);
    }

    public async Task<IReadOnlyList<MinutesByGuidRow>> GetMinutesByEmployeeAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildFilteredQueryAsync(
            from, to, projectId, employeeId, allowedEmployeeIds, completedOnly: true, cancellationToken);

        return await query
            .GroupBy(t => t.EmployeeId)
            .Select(g => new MinutesByGuidRow(g.Key, g.Sum(t => t.WorkedMinutes)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MinutesByGuidRow>> GetMinutesByProjectAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildFilteredQueryAsync(
            from, to, projectId, employeeId, allowedEmployeeIds, completedOnly: true, cancellationToken);

        return await query
            .GroupBy(t => t.ProjectId)
            .Select(g => new MinutesByGuidRow(g.Key, g.Sum(t => t.WorkedMinutes)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MinutesByDateRow>> GetMinutesByDayAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildFilteredQueryAsync(
            from, to, projectId, employeeId, allowedEmployeeIds, completedOnly: true, cancellationToken);
        var entries = await query
            .Select(t => new { t.StartTime, t.WorkedMinutes })
            .ToListAsync(cancellationToken);

        return entries
            .GroupBy(t => DateOnly.FromDateTime(t.StartTime))
            .Select(g => new MinutesByDateRow(g.Key, g.Sum(t => t.WorkedMinutes)))
            .OrderBy(r => r.Date)
            .ToList();
    }

    public async Task<IReadOnlyList<MinutesByMonthRow>> GetMinutesByMonthAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildFilteredQueryAsync(
            from, to, projectId, employeeId, allowedEmployeeIds, completedOnly: true, cancellationToken);
        var entries = await query
            .Select(t => new { t.StartTime, t.WorkedMinutes })
            .ToListAsync(cancellationToken);

        return entries
            .GroupBy(t => new { t.StartTime.Year, t.StartTime.Month })
            .Select(g => new MinutesByMonthRow(g.Key.Year, g.Key.Month, g.Sum(t => t.WorkedMinutes)))
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToList();
    }

    public async Task<int> GetOvertimeMinutesAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        int dailyStandardMinutes,
        CancellationToken cancellationToken = default)
    {
        var query = await BuildFilteredQueryAsync(
            from, to, projectId, employeeId, allowedEmployeeIds, completedOnly: true, cancellationToken);
        var entries = await query
            .Select(t => new { t.EmployeeId, t.StartTime, t.WorkedMinutes })
            .ToListAsync(cancellationToken);

        var overtime = 0;
        foreach (var group in entries.GroupBy(t => new { t.EmployeeId, Date = DateOnly.FromDateTime(t.StartTime) }))
        {
            var dailyMinutes = group.Sum(t => t.WorkedMinutes);
            if (dailyMinutes > dailyStandardMinutes)
                overtime += dailyMinutes - dailyStandardMinutes;
        }

        return overtime;
    }

    public async Task<IReadOnlyList<ActiveTimerAnalyticsRow>> GetActiveTimersAsync(
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.EndTime == null);

        query = ApplyEmployeeScope(query, employeeId, allowedEmployeeIds);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        return await query
            .Select(t => new ActiveTimerAnalyticsRow(t.EmployeeId, t.ProjectId, t.StartTime))
            .ToListAsync(cancellationToken);
    }

    private async Task<IQueryable<TimeEntry>> BuildFilteredQueryAsync(
        DateOnly from,
        DateOnly to,
        Guid? projectId,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        bool completedOnly,
        CancellationToken cancellationToken)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var approvedEntryIds = await _timesheetRepository.GetApprovedTimeEntryIdsAsync(
            employeeId, allowedEmployeeIds, from, to, cancellationToken);

        var query = _dbContext.Set<TimeEntry>()
            .ApplyTenantScope(_accessor.Current)
            .Where(t => t.StartTime >= fromUtc && t.StartTime < toUtc);

        if (completedOnly)
            query = query.Where(t => t.EndTime != null);

        if (approvedEntryIds.Count > 0)
            query = query.Where(t => approvedEntryIds.Contains(t.Id));
        else
            query = query.Where(t => false);

        query = ApplyEmployeeScope(query, employeeId, allowedEmployeeIds);

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        return query;
    }

    private static IQueryable<TimeEntry> ApplyEmployeeScope(
        IQueryable<TimeEntry> query,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds)
    {
        if (allowedEmployeeIds is not null)
            query = query.Where(t => allowedEmployeeIds.Contains(t.EmployeeId));

        if (employeeId.HasValue)
            query = query.Where(t => t.EmployeeId == employeeId.Value);

        return query;
    }
}
