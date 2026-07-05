using HrPortal.AccessControl.Domain;
using HrPortal.Tenancy;

namespace HrPortal.AccessControl.Application;

public interface IScopeResolver
{
    bool IsInScope(TenantContext ctx, AccessScope scope, ResourceContext resource);
}
