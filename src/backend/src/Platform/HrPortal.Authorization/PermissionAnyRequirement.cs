using Microsoft.AspNetCore.Authorization;

namespace HrPortal.Authorization;

/// <summary>Satisfied when the caller holds ANY of the listed permissions (OR semantics).</summary>
public sealed class PermissionAnyRequirement : IAuthorizationRequirement
{
    public PermissionAnyRequirement(IReadOnlyList<string> permissions) => Permissions = permissions;

    public IReadOnlyList<string> Permissions { get; }
}
