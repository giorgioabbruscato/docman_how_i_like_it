using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.TimeTracking.Domain;

public enum TimesheetStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}

public enum ApprovalDecision
{
    Approved,
    Rejected
}

public sealed class TimesheetSubmission : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public int TotalWorkedMinutes { get; private set; }
    public TimesheetStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? SubmittedAt { get; private set; }

    private readonly List<TimesheetSubmissionEntry> _entries = [];
    public IReadOnlyList<TimesheetSubmissionEntry> Entries => _entries.AsReadOnly();

    private TimesheetSubmission() { }

    public static TimesheetSubmission Create(
        Guid tenantId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        int totalWorkedMinutes,
        IReadOnlyList<Guid> timeEntryIds,
        string? notes = null,
        Guid? createdBy = null)
    {
        if (periodEnd < periodStart)
            throw new DomainException("Period end must be on or after period start.");

        var submission = new TimesheetSubmission
        {
            EmployeeId = employeeId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            TotalWorkedMinutes = totalWorkedMinutes,
            Status = TimesheetStatus.Draft,
            Notes = notes,
            CreatedBy = createdBy
        }.Also(s => s.SetTenant(tenantId));

        foreach (var entryId in timeEntryIds)
            submission._entries.Add(TimesheetSubmissionEntry.Create(tenantId, submission.Id, entryId));

        return submission;
    }

    public void Submit()
    {
        if (Status != TimesheetStatus.Draft)
            throw new DomainException("Only draft timesheets can be submitted.", "CONFLICT");

        Status = TimesheetStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        MarkUpdated(null);
    }

    public void Approve()
    {
        if (Status != TimesheetStatus.Submitted)
            throw new DomainException("Only submitted timesheets can be approved.", "CONFLICT");

        Status = TimesheetStatus.Approved;
        MarkUpdated(null);
    }

    public void Reject()
    {
        if (Status != TimesheetStatus.Submitted)
            throw new DomainException("Only submitted timesheets can be rejected.", "CONFLICT");

        Status = TimesheetStatus.Rejected;
        MarkUpdated(null);
    }
}

public sealed class TimesheetSubmissionEntry : AuditableEntity
{
    public Guid TimesheetSubmissionId { get; private set; }
    public Guid TimeEntryId { get; private set; }

    private TimesheetSubmissionEntry() { }

    internal static TimesheetSubmissionEntry Create(Guid tenantId, Guid submissionId, Guid timeEntryId) =>
        new TimesheetSubmissionEntry
        {
            TimesheetSubmissionId = submissionId,
            TimeEntryId = timeEntryId
        }.Also(e => e.SetTenant(tenantId));
}

public sealed class TimesheetApproval : AuditableEntity
{
    public Guid TimesheetSubmissionId { get; private set; }
    public Guid DecidedBy { get; private set; }
    public ApprovalDecision Decision { get; private set; }
    public string? Comment { get; private set; }
    public DateTime DecidedAt { get; private set; }

    private TimesheetApproval() { }

    public static TimesheetApproval Create(
        Guid tenantId,
        Guid timesheetSubmissionId,
        Guid decidedBy,
        ApprovalDecision decision,
        string? comment = null)
    {
        return new TimesheetApproval
        {
            TimesheetSubmissionId = timesheetSubmissionId,
            DecidedBy = decidedBy,
            Decision = decision,
            Comment = comment,
            DecidedAt = DateTime.UtcNow,
            CreatedBy = decidedBy
        }.Also(a => a.SetTenant(tenantId));
    }
}
