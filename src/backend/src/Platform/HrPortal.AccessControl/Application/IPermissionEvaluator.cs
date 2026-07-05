using HrPortal.Tenancy;

namespace HrPortal.AccessControl.Application;

public interface IPermissionEvaluator
{
    bool Evaluate(TenantContext ctx, string permission, ResourceContext? resource);
}
