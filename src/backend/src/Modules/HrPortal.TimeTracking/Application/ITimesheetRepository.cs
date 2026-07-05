using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;

namespace HrPortal.TimeTracking.Application;

public interface ITimesheetRepository
{
    Task<TimesheetSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TimesheetSubmission?> GetByIdWithEntriesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TimesheetSubmission>> GetPagedAsync(
        GetTimesheetsQuery query,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingPeriodAsync(
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeEntry>> GetCompletedEntriesForPeriodAsync(
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);
    Task<IReadOnlySet<Guid>> GetApprovedTimeEntryIdsAsync(
        Guid? employeeId,
        IReadOnlyList<Guid>? allowedEmployeeIds,
        DateOnly? fromDate,
        DateOnly? toDate,
        CancellationToken cancellationToken = default);
    Task<TimesheetApproval?> GetLatestApprovalAsync(Guid submissionId, CancellationToken cancellationToken = default);
    Task AddAsync(TimesheetSubmission submission, CancellationToken cancellationToken = default);
    Task AddApprovalAsync(TimesheetApproval approval, CancellationToken cancellationToken = default);
    Task UpdateAsync(TimesheetSubmission submission, CancellationToken cancellationToken = default);
}
