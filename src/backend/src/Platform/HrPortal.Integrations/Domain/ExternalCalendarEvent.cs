using HrPortal.SharedKernel.Entities;

namespace HrPortal.Integrations.Domain;

public sealed class ExternalCalendarEvent : AuditableEntity
{
    public Guid LeaveRequestId { get; private set; }
    public CalendarProvider Provider { get; private set; }
    public string ExternalEventId { get; private set; } = string.Empty;
    public DateTime LastSyncedAt { get; private set; }

    private ExternalCalendarEvent() { }

    public static ExternalCalendarEvent Create(
        Guid tenantId,
        Guid leaveRequestId,
        CalendarProvider provider,
        string externalEventId)
    {
        return new ExternalCalendarEvent
        {
            LeaveRequestId = leaveRequestId,
            Provider = provider,
            ExternalEventId = externalEventId,
            LastSyncedAt = DateTime.UtcNow
        }.Also(e => e.SetTenant(tenantId));
    }

    public void UpdateSync(string externalEventId)
    {
        ExternalEventId = externalEventId;
        LastSyncedAt = DateTime.UtcNow;
        MarkUpdated(null);
    }
}
