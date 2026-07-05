using Microsoft.AspNetCore.Authorization;

namespace HrPortal.Authorization;

/// <summary>
/// Grants access when the caller holds ANY of the given permissions. Required for endpoints where
/// different roles are authorized at different scopes (e.g. manager team-scope vs HR tenant-scope)
/// and no single resource-aware permission would cover both.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireAnyPermissionAttribute : AuthorizeAttribute
{
    public RequireAnyPermissionAttribute(params string[] permissions)
        : base(Policies.PermissionAnyPrefix + string.Join(Policies.PermissionAnySeparator, permissions)) { }
}
