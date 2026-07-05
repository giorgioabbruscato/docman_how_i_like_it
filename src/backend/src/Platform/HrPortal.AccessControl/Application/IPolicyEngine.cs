using HrPortal.Tenancy;

namespace HrPortal.AccessControl.Application;

public interface IPolicyEngine
{
    bool Can(TenantContext ctx, string permission, ResourceContext? resource);
}
