namespace HrPortal.Authorization;

public static class Policies
{
    public const string PermissionPrefix = "Permission:";
    public const string PermissionAnyPrefix = "PermissionAny:";
    public const string PermissionAnySeparator = "|";

    [Obsolete("Deprecated in task 23 — migrate to [RequirePermission] / [RequireAnyPermission]. Retained only for the legacy-user migration shim.")]
    public const string AdminOnly = "AdminOnly";

    [Obsolete("Deprecated in task 23 — migrate to [RequirePermission] / [RequireAnyPermission]. Retained only for the legacy-user migration shim.")]
    public const string HrOrAdmin = "HrOrAdmin";

    [Obsolete("Deprecated in task 23 — migrate to [RequirePermission] / [RequireAnyPermission]. Retained only for the legacy-user migration shim.")]
    public const string ManagerOrAbove = "ManagerOrAbove";

    public const string Authenticated = "Authenticated";
}
