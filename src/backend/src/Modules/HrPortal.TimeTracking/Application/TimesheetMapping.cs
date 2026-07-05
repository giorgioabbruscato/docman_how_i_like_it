using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;

namespace HrPortal.TimeTracking.Application;

internal static class TimesheetMapping
{
    public static TimesheetSubmissionDto ToDto(
        TimesheetSubmission submission,
        TimesheetApproval? latestApproval = null) =>
        new(
            submission.Id,
            submission.EmployeeId,
            submission.PeriodStart,
            submission.PeriodEnd,
            submission.TotalWorkedMinutes,
            submission.Status.ToString(),
            submission.Notes,
            submission.SubmittedAt,
            submission.Entries.Select(e => e.TimeEntryId).ToList(),
            latestApproval is null ? null : ToApprovalDto(latestApproval));

    public static TimesheetApprovalDto ToApprovalDto(TimesheetApproval approval) =>
        new(
            approval.Id,
            approval.DecidedBy,
            approval.Decision.ToString(),
            approval.Comment,
            approval.DecidedAt);
}
