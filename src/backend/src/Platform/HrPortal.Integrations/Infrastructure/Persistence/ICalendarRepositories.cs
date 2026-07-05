using HrPortal.Integrations.Domain;

namespace HrPortal.Integrations.Infrastructure.Persistence;

public interface ICalendarConnectionRepository
{
    Task<CalendarConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalendarConnection>> GetActiveByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
    Task<CalendarConnection?> GetByEmployeeAndProviderAsync(
        Guid employeeId,
        CalendarProvider provider,
        CancellationToken cancellationToken = default);
    Task AddAsync(CalendarConnection connection, CancellationToken cancellationToken = default);
    Task UpdateAsync(CalendarConnection connection, CancellationToken cancellationToken = default);
}

public interface IExternalCalendarEventRepository
{
    Task<ExternalCalendarEvent?> GetByLeaveAndProviderAsync(
        Guid leaveRequestId,
        CalendarProvider provider,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalCalendarEvent>> GetByLeaveRequestAsync(
        Guid leaveRequestId,
        CancellationToken cancellationToken = default);
    Task AddAsync(ExternalCalendarEvent calendarEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExternalCalendarEvent calendarEvent, CancellationToken cancellationToken = default);
    Task DeleteAsync(ExternalCalendarEvent calendarEvent, CancellationToken cancellationToken = default);
}

public interface ICalendarSyncLogRepository
{
    Task<IReadOnlyList<CalendarSyncLog>> GetRecentAsync(
        int limit,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CalendarSyncLog>> GetPendingRetriesAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default);
    Task AddAsync(CalendarSyncLog log, CancellationToken cancellationToken = default);
    Task UpdateAsync(CalendarSyncLog log, CancellationToken cancellationToken = default);
}
