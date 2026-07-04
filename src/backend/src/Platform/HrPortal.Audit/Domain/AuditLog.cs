using HrPortal.SharedKernel.Entities;

namespace HrPortal.Audit.Domain;

public sealed class AuditLog : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Entity { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? Metadata { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid tenantId,
        Guid userId,
        string action,
        string entity,
        string? entityId = null,
        string? metadata = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Metadata = metadata
        };
    }
}
