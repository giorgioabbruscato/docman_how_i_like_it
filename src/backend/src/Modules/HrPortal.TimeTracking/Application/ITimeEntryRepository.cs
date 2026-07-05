using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;

namespace HrPortal.TimeTracking.Application;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TimeEntry>> GetPagedAsync(
        GetTimeEntriesQuery query,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeEntry>> GetForExportAsync(
        ExportTimeEntriesQuery query,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);
    Task<TimeEntry?> GetActiveTimerAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingEntryAsync(
        Guid employeeId,
        DateTime start,
        DateTime? end,
        Guid? excludeEntryId = null,
        CancellationToken cancellationToken = default);
    Task<int> GetTotalMinutesForDateAsync(
        Guid employeeId,
        DateOnly date,
        Guid? excludeEntryId = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(TimeEntry entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(TimeEntry entry, CancellationToken cancellationToken = default);
    Task DeleteAsync(TimeEntry entry, CancellationToken cancellationToken = default);
}
