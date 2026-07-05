using System.Text.Json;
using HrPortal.Audit.Application;
using HrPortal.Audit.Domain;
using HrPortal.Identity;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HrPortal.Audit.Infrastructure;

internal sealed class AuditService : IAuditService
{
    private readonly DbContext _dbContext;
    private readonly TenantContext _tenantContext;
    private readonly UserContext _userContext;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        DbContext dbContext,
        TenantContext tenantContext,
        UserContext userContext,
        ILogger<AuditService> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.IsResolved)
        {
            _logger.LogWarning("Skipping audit log: tenant not resolved");
            return;
        }

        await AddAuditLogAsync(
            _tenantContext.TenantId,
            _userContext.IsAuthenticated ? _userContext.UserId : Guid.Empty,
            entry,
            cancellationToken);
    }

    public Task LogForTenantAsync(
        Guid tenantId,
        AuditEntry entry,
        CancellationToken cancellationToken = default) =>
        AddAuditLogAsync(
            tenantId,
            _userContext.IsAuthenticated ? _userContext.UserId : Guid.Empty,
            entry,
            cancellationToken);

    public async Task LogAccessDecisionAsync(
        AccessDecisionEntry entry,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.IsResolved)
        {
            _logger.LogWarning("Skipping access decision audit: tenant not resolved");
            return;
        }

        var metadata = JsonSerializer.Serialize(new
        {
            entry.Permission,
            entry.Allowed,
            entry.IpAddress,
            resource = new
            {
                employeeId = entry.ResourceEmployeeId,
                departmentId = entry.ResourceDepartmentId,
                tenantId = entry.ResourceTenantId
            }
        });

        await AddAuditLogAsync(
            _tenantContext.TenantId,
            entry.ActorUserId ?? (_userContext.IsAuthenticated ? _userContext.UserId : Guid.Empty),
            new AuditEntry(
                entry.Allowed ? "access.allowed" : "access.denied",
                "Authorization",
                entry.Permission,
                metadata),
            cancellationToken,
            actorEmail: _userContext.IsAuthenticated ? _userContext.Email : null,
            ipAddress: entry.IpAddress,
            decision: entry.Allowed ? AuditDecision.Allow : AuditDecision.Deny,
            scope: ResolveScope(entry.Permission),
            targetId: ResolveTargetId(entry),
            // Access decisions are logged by the authorization handler, which runs before the MVC action
            // and its own IUnitOfWork.SaveChangesAsync — nothing downstream would otherwise persist them
            // (e.g. GET requests never call SaveChangesAsync), so they must be committed immediately.
            saveImmediately: true);
    }

    private async Task AddAuditLogAsync(
        Guid tenantId,
        Guid userId,
        AuditEntry entry,
        CancellationToken cancellationToken,
        string? actorEmail = null,
        string? ipAddress = null,
        string? decision = null,
        string? scope = null,
        string? targetId = null,
        bool saveImmediately = false)
    {
        var auditLog = AuditLog.Create(
            tenantId,
            userId,
            entry.Action,
            entry.Entity,
            entry.EntityId,
            entry.Metadata,
            actorEmail,
            ipAddress,
            decision,
            scope,
            targetId);

        await _dbContext.Set<AuditLog>().AddAsync(auditLog, cancellationToken);

        if (saveImmediately)
            await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? ResolveScope(string permission)
    {
        var separatorIndex = permission.LastIndexOf(':');
        return separatorIndex >= 0 && separatorIndex < permission.Length - 1
            ? permission[(separatorIndex + 1)..]
            : null;
    }

    private static string? ResolveTargetId(AccessDecisionEntry entry) =>
        entry.ResourceEmployeeId?.ToString()
        ?? entry.ResourceDepartmentId?.ToString()
        ?? entry.ResourceTenantId?.ToString();
}
