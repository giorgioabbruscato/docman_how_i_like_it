using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Tenancy;

public static class TenantQueryExtensions
{
    public static IQueryable<TEntity> ApplyTenantScope<TEntity>(
        this IQueryable<TEntity> query,
        ITenantContext ctx)
        where TEntity : class, ITenantEntity
    {
        if (ctx.Mode == TenantDeploymentMode.Single)
            return query;

        if (!ctx.IsResolved)
            throw new TenantNotResolvedException();

        return query.Where(e => e.TenantId == ctx.TenantId);
    }
}
