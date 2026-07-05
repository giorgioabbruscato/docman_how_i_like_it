namespace HrPortal.Audit.Application;

public sealed record AuditEntry(
    string Action,
    string Entity,
    string? EntityId = null,
    string? Metadata = null);

public sealed record AccessDecisionEntry(
    Guid? ActorUserId,
    string Permission,
    bool Allowed,
    string? IpAddress = null,
    Guid? ResourceEmployeeId = null,
    Guid? ResourceDepartmentId = null,
    Guid? ResourceTenantId = null);

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
    Task LogForTenantAsync(Guid tenantId, AuditEntry entry, CancellationToken cancellationToken = default);
    Task LogAccessDecisionAsync(AccessDecisionEntry entry, CancellationToken cancellationToken = default);
}
