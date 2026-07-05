namespace HrPortal.Leave.Application;

/// <summary>
/// Hook for external calendar sync after leave workflow completion.
/// Implemented by HrPortal.Integrations; no-op when integrations are disabled.
/// </summary>
public interface ILeaveCalendarSyncService
{
    Task SyncApprovedAsync(Guid leaveRequestId, CancellationToken cancellationToken = default);

    Task DeleteEventsAsync(Guid leaveRequestId, CancellationToken cancellationToken = default);
}
