namespace HrPortal.Authorization;

public static class Policies
{
    public const string PermissionPrefix = "Permission:";
    public const string AdminOnly = "AdminOnly";
    public const string HrOrAdmin = "HrOrAdmin";
    public const string ManagerOrAbove = "ManagerOrAbove";
    public const string Authenticated = "Authenticated";
}
