namespace HrPortal.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Hr = "HR";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static readonly IReadOnlyList<string> All = [Admin, Hr, Manager, Employee];
}
