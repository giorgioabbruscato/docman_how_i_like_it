using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Attendance.Infrastructure.Analytics;

internal sealed class AttendanceAnalyticsProvider : IAttendanceAnalyticsProvider
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public AttendanceAnalyticsProvider(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<IReadOnlyList<AttendanceSessionAnalyticsRow>> GetOpenSessionsAsync(
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<AttendanceSession>()
            .ApplyTenantScope(_accessor.Current)
            .Where(s => s.Status == AttendanceSessionStatus.Open);

        query = ApplyEmployeeScope(query, null, allowedEmployeeIds);

        return await query
            .Select(s => new AttendanceSessionAnalyticsRow(
                s.EmployeeId,
                s.CheckIn,
                s.CheckOut,
                true))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceSessionAnalyticsRow>> GetSessionsInRangeAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var fromUtc = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toUtc = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var query = _dbContext.Set<AttendanceSession>()
            .ApplyTenantScope(_accessor.Current)
            .Where(s => s.CheckIn >= fromUtc && s.CheckIn < toUtc);

        query = ApplyEmployeeScope(query, employeeId, allowedEmployeeIds);

        return await query
            .Select(s => new AttendanceSessionAnalyticsRow(
                s.EmployeeId,
                s.CheckIn,
                s.CheckOut,
                s.Status == AttendanceSessionStatus.Open))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceSessionAnalyticsRow>> GetLateCheckInsAsync(
        DateOnly date,
        TimeOnly lateThreshold,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var threshold = date.ToDateTime(lateThreshold, DateTimeKind.Utc);

        var query = _dbContext.Set<AttendanceSession>()
            .ApplyTenantScope(_accessor.Current)
            .Where(s => s.CheckIn >= dayStart && s.CheckIn < dayEnd && s.CheckIn > threshold);

        query = ApplyEmployeeScope(query, null, allowedEmployeeIds);

        return await query
            .Select(s => new AttendanceSessionAnalyticsRow(
                s.EmployeeId,
                s.CheckIn,
                s.CheckOut,
                s.Status == AttendanceSessionStatus.Open))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetPresentEmployeeDaysAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var sessions = await GetSessionsInRangeAsync(from, to, employeeId, allowedEmployeeIds, cancellationToken);

        return sessions
            .Select(s => new { s.EmployeeId, Date = DateOnly.FromDateTime(s.CheckIn) })
            .Distinct()
            .Count();
    }

    public async Task<IReadOnlyList<PresentEmployeeDayRow>> GetDailyPresentCountsAsync(
        DateOnly from,
        DateOnly to,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default)
    {
        var sessions = await GetSessionsInRangeAsync(from, to, employeeId, allowedEmployeeIds, cancellationToken);

        return sessions
            .GroupBy(s => DateOnly.FromDateTime(s.CheckIn))
            .Select(g => new PresentEmployeeDayRow(g.Key, g.Select(s => s.EmployeeId).Distinct().Count()))
            .OrderBy(r => r.Date)
            .ToList();
    }

    private static IQueryable<AttendanceSession> ApplyEmployeeScope(
        IQueryable<AttendanceSession> query,
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds)
    {
        if (allowedEmployeeIds is not null)
            query = query.Where(s => allowedEmployeeIds.Contains(s.EmployeeId));

        if (employeeId.HasValue)
            query = query.Where(s => s.EmployeeId == employeeId.Value);

        return query;
    }
}
