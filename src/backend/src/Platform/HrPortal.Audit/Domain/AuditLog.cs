using HrPortal.SharedKernel.Entities;

namespace HrPortal.Audit.Domain;

/// <summary>Canonical access-decision outcomes for <see cref="AuditLog.Decision"/>.</summary>
public static class AuditDecision
{
    public const string Allow = "Allow";
    public const string Deny = "Deny";
}

public sealed class AuditLog : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Entity { get; private set; } = string.Empty;
    public string? EntityId { get; private set; }
    public string? Metadata { get; private set; }

    /// <summary>Identifier of the resource the action/decision targeted (employee, department, tenant, etc).</summary>
    public string? TargetId { get; private set; }

    /// <summary>Resource scope the decision was evaluated against (self/team/department/tenant/all).</summary>
    public string? Scope { get; private set; }

    public string? IpAddress { get; private set; }
    public string? ActorEmail { get; private set; }

    /// <summary>Allow/Deny for access-decision entries; null for plain business-mutation audit entries.</summary>
    public string? Decision { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid tenantId,
        Guid userId,
        string action,
        string entity,
        string? entityId = null,
        string? metadata = null,
        string? actorEmail = null,
        string? ipAddress = null,
        string? decision = null,
        string? scope = null,
        string? targetId = null)
    {
        return new AuditLog
        {
            TenantId = tenantId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Metadata = metadata,
            ActorEmail = actorEmail,
            IpAddress = ipAddress,
            Decision = decision,
            Scope = scope,
            TargetId = targetId ?? entityId
        };
    }
}
