using HrPortal.Leave.Application;
using HrPortal.Leave.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Leave.Infrastructure.Analytics;

internal sealed class LeaveAnalyticsProvider : ILeaveAnalyticsProvider
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public LeaveAnalyticsProvider(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<int> GetApprovedLeaveDaysAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var requests = await GetApprovedRequestsInRangeAsync(from, to, employeeId, allowedEmployeeIds, cancellationToken);
        return requests.Sum(r => CountOverlapDays(r.StartDate, r.EndDate, from, to));
    }

    public async Task<IReadOnlyList<LeaveDaysByMonthRow>> GetMonthlyLeaveTrendAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var requests = await GetApprovedRequestsInRangeAsync(from, to, employeeId, allowedEmployeeIds, cancellationToken);
        var monthDays = new Dictionary<(int Year, int Month), int>();

        foreach (var request in requests)
        {
            var overlapStart = request.StartDate > from ? request.StartDate : from;
            var overlapEnd = request.EndDate < to ? request.EndDate : to;

            for (var date = overlapStart; date <= overlapEnd; date = date.AddDays(1))
            {
                var key = (date.Year, date.Month);
                monthDays.TryGetValue(key, out var current);
                monthDays[key] = current + 1;
            }
        }

        return monthDays
            .Select(kvp => new LeaveDaysByMonthRow(kvp.Key.Year, kvp.Key.Month, kvp.Value))
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToList();
    }

    private async Task<IReadOnlyList<LeaveRequest>> GetApprovedRequestsInRangeAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<LeaveRequest>()
            .ApplyTenantScope(_accessor.Current)
            .Where(l => l.Status == LeaveStatus.Approved)
            .Where(l => l.StartDate <= to && l.EndDate >= from);

        if (allowedEmployeeIds is not null)
            query = query.Where(l => allowedEmployeeIds.Contains(l.EmployeeId));

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);

        return await query.ToListAsync(cancellationToken);
    }

    private static int CountOverlapDays(DateOnly start, DateOnly end, DateOnly rangeFrom, DateOnly rangeTo)
    {
        var overlapStart = start > rangeFrom ? start : rangeFrom;
        var overlapEnd = end < rangeTo ? end : rangeTo;

        if (overlapEnd < overlapStart)
            return 0;

        return overlapEnd.DayNumber - overlapStart.DayNumber + 1;
    }
}
