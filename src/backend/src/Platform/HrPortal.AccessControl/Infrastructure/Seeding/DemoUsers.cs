namespace HrPortal.AccessControl.Infrastructure.Seeding;

/// <summary>Deterministic user IDs for demo seed and integration tests.</summary>
public static class DemoUsers
{
    public static readonly Guid Admin = Guid.Parse("11111111-1111-4111-8111-111111111101");
    public static readonly Guid Hr = Guid.Parse("11111111-1111-4111-8111-111111111102");
    public static readonly Guid Employee = Guid.Parse("11111111-1111-4111-8111-111111111103");
    public static readonly Guid PlatformAdmin = Guid.Parse("11111111-1111-4111-8111-111111111104");

    public const string AdminEmail = "admin@demo.local";
    public const string HrEmail = "hr@demo.local";
    public const string EmployeeEmail = "employee@demo.local";
    public const string PlatformAdminEmail = "platform.owner@demo.local";
}
