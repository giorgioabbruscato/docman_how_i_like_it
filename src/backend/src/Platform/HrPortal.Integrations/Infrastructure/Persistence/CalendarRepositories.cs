using HrPortal.Integrations.Domain;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Integrations.Infrastructure.Persistence;

internal sealed class CalendarConnectionRepository : ICalendarConnectionRepository
{
    private readonly DbContext _dbContext;

    public CalendarConnectionRepository(DbContext dbContext) => _dbContext = dbContext;

    public Task<CalendarConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Set<CalendarConnection>().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CalendarConnection>> GetActiveByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<CalendarConnection>()
            .Where(c => c.EmployeeId == employeeId && c.IsActive)
            .ToListAsync(cancellationToken);

    public Task<CalendarConnection?> GetByEmployeeAndProviderAsync(
        Guid employeeId,
        CalendarProvider provider,
        CancellationToken cancellationToken = default) =>
        _dbContext.Set<CalendarConnection>()
            .FirstOrDefaultAsync(c => c.EmployeeId == employeeId && c.Provider == provider, cancellationToken);

    public async Task AddAsync(CalendarConnection connection, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<CalendarConnection>().AddAsync(connection, cancellationToken);

    public Task UpdateAsync(CalendarConnection connection, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<CalendarConnection>().Update(connection);
        return Task.CompletedTask;
    }
}

internal sealed class ExternalCalendarEventRepository : IExternalCalendarEventRepository
{
    private readonly DbContext _dbContext;

    public ExternalCalendarEventRepository(DbContext dbContext) => _dbContext = dbContext;

    public Task<ExternalCalendarEvent?> GetByLeaveAndProviderAsync(
        Guid leaveRequestId,
        CalendarProvider provider,
        CancellationToken cancellationToken = default) =>
        _dbContext.Set<ExternalCalendarEvent>()
            .FirstOrDefaultAsync(e => e.LeaveRequestId == leaveRequestId && e.Provider == provider, cancellationToken);

    public async Task<IReadOnlyList<ExternalCalendarEvent>> GetByLeaveRequestAsync(
        Guid leaveRequestId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ExternalCalendarEvent>()
            .Where(e => e.LeaveRequestId == leaveRequestId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ExternalCalendarEvent calendarEvent, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<ExternalCalendarEvent>().AddAsync(calendarEvent, cancellationToken);

    public Task UpdateAsync(ExternalCalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ExternalCalendarEvent>().Update(calendarEvent);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ExternalCalendarEvent calendarEvent, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ExternalCalendarEvent>().Remove(calendarEvent);
        return Task.CompletedTask;
    }
}

internal sealed class CalendarSyncLogRepository : ICalendarSyncLogRepository
{
    private readonly DbContext _dbContext;

    public CalendarSyncLogRepository(DbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<CalendarSyncLog>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<CalendarSyncLog>()
            .OrderByDescending(l => l.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CalendarSyncLog>> GetPendingRetriesAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<CalendarSyncLog>()
            .Where(l => l.Status == CalendarSyncStatus.PendingRetry
                        && l.NextRetryAt != null
                        && l.NextRetryAt <= asOf)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CalendarSyncLog log, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<CalendarSyncLog>().AddAsync(log, cancellationToken);

    public Task UpdateAsync(CalendarSyncLog log, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<CalendarSyncLog>().Update(log);
        return Task.CompletedTask;
    }
}
