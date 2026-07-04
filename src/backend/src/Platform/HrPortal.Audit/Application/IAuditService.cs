namespace HrPortal.Audit.Application;

public sealed record AuditEntry(
    string Action,
    string Entity,
    string? EntityId = null,
    string? Metadata = null);

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
