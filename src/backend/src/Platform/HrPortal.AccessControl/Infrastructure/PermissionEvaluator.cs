using HrPortal.AccessControl.Application;
using HrPortal.Tenancy;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class PermissionEvaluator : IPermissionEvaluator
{
    private readonly IPolicyEngine _policyEngine;

    public PermissionEvaluator(IPolicyEngine policyEngine) => _policyEngine = policyEngine;

    public bool Evaluate(TenantContext ctx, string permission, ResourceContext? resource) =>
        _policyEngine.Can(ctx, permission, resource);
}
