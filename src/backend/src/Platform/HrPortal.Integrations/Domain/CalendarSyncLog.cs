using HrPortal.SharedKernel.Entities;

namespace HrPortal.Integrations.Domain;

public enum CalendarSyncStatus
{
    Success,
    Failed,
    PendingRetry
}

public sealed class CalendarSyncLog : AuditableEntity
{
    public Guid LeaveRequestId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public CalendarProvider? Provider { get; private set; }
    public CalendarSyncStatus Status { get; private set; }
    public string? Message { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? NextRetryAt { get; private set; }

    private CalendarSyncLog() { }

    public static CalendarSyncLog CreateSuccess(
        Guid tenantId,
        Guid leaveRequestId,
        Guid employeeId,
        CalendarProvider provider,
        string? message = null)
    {
        return new CalendarSyncLog
        {
            LeaveRequestId = leaveRequestId,
            EmployeeId = employeeId,
            Provider = provider,
            Status = CalendarSyncStatus.Success,
            Message = message
        }.Also(l => l.SetTenant(tenantId));
    }

    public static CalendarSyncLog CreateFailure(
        Guid tenantId,
        Guid leaveRequestId,
        Guid? employeeId,
        CalendarProvider? provider,
        string message,
        int retryCount = 0,
        DateTime? nextRetryAt = null)
    {
        return new CalendarSyncLog
        {
            LeaveRequestId = leaveRequestId,
            EmployeeId = employeeId,
            Provider = provider,
            Status = retryCount > 0 ? CalendarSyncStatus.PendingRetry : CalendarSyncStatus.Failed,
            Message = message,
            RetryCount = retryCount,
            NextRetryAt = nextRetryAt
        }.Also(l => l.SetTenant(tenantId));
    }

    public void MarkRetried(string message, int retryCount, DateTime? nextRetryAt)
    {
        Message = message;
        RetryCount = retryCount;
        NextRetryAt = nextRetryAt;
        Status = CalendarSyncStatus.PendingRetry;
        MarkUpdated(null);
    }

    public void MarkSuccess(string? message = null)
    {
        Status = CalendarSyncStatus.Success;
        Message = message;
        NextRetryAt = null;
        MarkUpdated(null);
    }
}
