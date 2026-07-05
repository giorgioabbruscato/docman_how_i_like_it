using HrPortal.SharedKernel.Results;

namespace HrPortal.Audit.Application;

public sealed record AuditLogQuery(
    DateTime? From = null,
    DateTime? To = null,
    Guid? ActorUserId = null,
    string? Action = null,
    string? Decision = null,
    int Page = 1,
    int PageSize = 50);

public sealed record AuditLogDto(
    Guid Id,
    DateTime Timestamp,
    Guid UserId,
    string? ActorEmail,
    string Action,
    string Entity,
    string? EntityId,
    string? TargetId,
    string? Scope,
    string? Decision,
    string? IpAddress,
    string? Metadata);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Filtered, paginated audit log querying. Gated at the controller by <c>audit.read:tenant</c> permission
/// and (per Task 25) the tenant's <c>auditLog</c> plan feature — Free-plan tenants get a feature-disabled
/// response even if they somehow hold the permission.
/// </summary>
public interface IAuditQueryService
{
    Task<Result<PagedResult<AuditLogDto>>> QueryAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default);
}
