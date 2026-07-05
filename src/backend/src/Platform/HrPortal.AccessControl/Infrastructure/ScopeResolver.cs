using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.Tenancy;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class ScopeResolver : IScopeResolver
{
    public bool IsInScope(TenantContext ctx, AccessScope scope, ResourceContext resource) =>
        scope switch
        {
            AccessScope.Self => ctx.EmployeeId.HasValue &&
                                resource.EmployeeId.HasValue &&
                                ctx.EmployeeId == resource.EmployeeId,
            AccessScope.Department => ctx.DepartmentId.HasValue &&
                                      resource.DepartmentId.HasValue &&
                                      ctx.DepartmentId == resource.DepartmentId,
            AccessScope.Team => IsTeamScope(ctx, resource),
            AccessScope.Tenant => resource.TenantId.HasValue &&
                                  resource.TenantId == ctx.TenantId,
            AccessScope.All => ctx.IsPlatformAdmin,
            _ => false
        };

    private static bool IsTeamScope(TenantContext ctx, ResourceContext resource)
    {
        if (ctx.EmployeeId.HasValue &&
            resource.EmployeeId.HasValue &&
            ctx.EmployeeId == resource.EmployeeId)
        {
            return true;
        }

        return ctx.DepartmentId.HasValue &&
               resource.DepartmentId.HasValue &&
               ctx.DepartmentId == resource.DepartmentId;
    }
}
