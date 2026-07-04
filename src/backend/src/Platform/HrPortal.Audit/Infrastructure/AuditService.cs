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

        var auditLog = AuditLog.Create(
            _tenantContext.TenantId,
            _userContext.IsAuthenticated ? _userContext.UserId : Guid.Empty,
            entry.Action,
            entry.Entity,
            entry.EntityId,
            entry.Metadata);

        await _dbContext.Set<AuditLog>().AddAsync(auditLog, cancellationToken);
    }
}
