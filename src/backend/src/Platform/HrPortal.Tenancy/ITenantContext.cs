namespace HrPortal.Tenancy;

public interface ITenantContext
{
    Guid TenantId { get; }
    string TenantSlug { get; }
    Guid? UserId { get; }
    string? Email { get; }
    TenantDeploymentMode Mode { get; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> RoleSlugs { get; }
    IReadOnlyList<string> Permissions { get; }
    Guid? EmployeeId { get; }
    Guid? DepartmentId { get; }
    IReadOnlyDictionary<string, string> Attributes { get; }
    IReadOnlyList<string> Features { get; }
    bool IsPlatformAdmin { get; }
    bool IsResolved { get; }

    bool HasPermission(string permission);
}
