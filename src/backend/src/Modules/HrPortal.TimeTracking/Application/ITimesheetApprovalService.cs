using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.TimeTracking.Application;

/// <summary>Replaceable approval workflow hook (Task 29 can swap implementation).</summary>
public interface ITimesheetApprovalService
{
    Task<Result<TimesheetSubmissionDto>> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<TimesheetSubmissionDto>> RejectAsync(
        Guid id,
        RejectTimesheetRequest request,
        CancellationToken cancellationToken = default);
}
