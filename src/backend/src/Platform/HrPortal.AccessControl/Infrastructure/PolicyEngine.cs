using HrPortal.AccessControl.Application;
using HrPortal.AccessControl.Domain;
using HrPortal.Tenancy;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class PolicyEngine : IPolicyEngine
{
    private readonly IScopeResolver _scopeResolver;

    public PolicyEngine(IScopeResolver scopeResolver) => _scopeResolver = scopeResolver;

    public bool Can(TenantContext ctx, string permission, ResourceContext? resource)
    {
        if (!ctx.HasPermission(permission))
            return false;

        if (resource is null)
            return true;

        if (!TryParseScope(permission, out var scope))
            return false;

        return _scopeResolver.IsInScope(ctx, scope, resource);
    }

    internal static bool TryParseScope(string permission, out AccessScope scope)
    {
        scope = default;
        var separatorIndex = permission.LastIndexOf(':');
        if (separatorIndex < 0 || separatorIndex == permission.Length - 1)
            return false;

        var scopeName = permission[(separatorIndex + 1)..];
        return Enum.TryParse(scopeName, ignoreCase: true, out scope);
    }
}
