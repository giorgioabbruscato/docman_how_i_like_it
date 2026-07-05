using HrPortal.Audit.Application;
using HrPortal.Audit.Domain;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Audit.Infrastructure;

internal sealed class AuditQueryService : IAuditQueryService
{
    private const int MaxPageSize = 200;

    private readonly DbContext _dbContext;
    private readonly TenantContext _tenantContext;

    public AuditQueryService(DbContext dbContext, TenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<AuditLogDto>>> QueryAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.IsResolved)
            return Result.Failure<PagedResult<AuditLogDto>>("Tenant context is not resolved.", "VALIDATION_ERROR");

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > MaxPageSize ? MaxPageSize : query.PageSize;

        var records = _dbContext.Set<AuditLog>()
            .ApplyTenantScope(_tenantContext)
            .AsQueryable();

        if (query.From.HasValue)
            records = records.Where(a => a.Timestamp >= query.From.Value);

        if (query.To.HasValue)
            records = records.Where(a => a.Timestamp <= query.To.Value);

        if (query.ActorUserId.HasValue)
            records = records.Where(a => a.UserId == query.ActorUserId.Value);

        if (!string.IsNullOrWhiteSpace(query.Action))
            records = records.Where(a => a.Action == query.Action);

        if (!string.IsNullOrWhiteSpace(query.Decision))
            records = records.Where(a => a.Decision == query.Decision);

        var totalCount = await records.CountAsync(cancellationToken);

        var items = await records
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto(
                a.Id,
                a.Timestamp,
                a.UserId,
                a.ActorEmail,
                a.Action,
                a.Entity,
                a.EntityId,
                a.TargetId,
                a.Scope,
                a.Decision,
                a.IpAddress,
                a.Metadata))
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<AuditLogDto>(items, totalCount, page, pageSize));
    }
}
