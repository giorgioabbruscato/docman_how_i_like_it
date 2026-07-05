using HrPortal.Attendance.Application;
using HrPortal.Attendance.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Attendance.Infrastructure.Persistence;

internal sealed class AttendanceSessionRepository : IAttendanceSessionRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public AttendanceSessionRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<AttendanceSession?> GetOpenSessionAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceSession>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(
                s => s.EmployeeId == employeeId && s.Status == AttendanceSessionStatus.Open,
                cancellationToken);

    public async Task<AttendanceSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceSession>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AttendanceSession>> GetByEmployeeAndDateRangeAsync(
        Guid employeeId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default) =>
        await ApplyDateRange(
                _dbContext.Set<AttendanceSession>()
                    .ApplyTenantScope(_accessor.Current)
                    .Where(s => s.EmployeeId == employeeId),
                from,
                to)
            .OrderByDescending(s => s.CheckIn)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AttendanceSession>> GetByEmployeeIdsAndDateRangeAsync(
        IReadOnlyList<Guid> employeeIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default) =>
        await ApplyDateRange(
                _dbContext.Set<AttendanceSession>()
                    .ApplyTenantScope(_accessor.Current)
                    .Where(s => employeeIds.Contains(s.EmployeeId)),
                from,
                to)
            .OrderByDescending(s => s.CheckIn)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<AttendanceSession> Items, int Total)> GetHistoryAsync(
        AttendanceSessionReadFilter filter,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(
            _dbContext.Set<AttendanceSession>()
                .ApplyTenantScope(_accessor.Current)
                .AsQueryable(),
            filter);

        if (from.HasValue)
            query = query.Where(s => s.CheckIn >= from.Value);

        if (to.HasValue)
            query = query.Where(s => s.CheckIn <= to.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.CheckIn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task AddAsync(AttendanceSession session, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<AttendanceSession>().AddAsync(session, cancellationToken);

    public Task UpdateAsync(AttendanceSession session, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<AttendanceSession>().Update(session);
        return Task.CompletedTask;
    }

    private static IQueryable<AttendanceSession> ApplyDateRange(
        IQueryable<AttendanceSession> query,
        DateTime from,
        DateTime to) =>
        query.Where(s => s.CheckIn >= from && s.CheckIn < to);

    private static IQueryable<AttendanceSession> ApplyFilter(
        IQueryable<AttendanceSession> query,
        AttendanceSessionReadFilter filter)
    {
        if (filter.EmployeeId.HasValue)
            return query.Where(s => s.EmployeeId == filter.EmployeeId.Value);

        if (filter.AllowedEmployeeIds is not null)
            return query.Where(s => filter.AllowedEmployeeIds.Contains(s.EmployeeId));

        return query;
    }
}
