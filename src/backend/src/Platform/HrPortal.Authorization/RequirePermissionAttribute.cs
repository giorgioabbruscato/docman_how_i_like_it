using Microsoft.AspNetCore.Authorization;

namespace HrPortal.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
        : base(Policies.PermissionPrefix + permission) { }
}
