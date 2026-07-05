namespace HrPortal.Tenancy;

public sealed record TenantContext(
    Guid TenantId,
    string TenantSlug,
    Guid? UserId = null,
    string? Email = null,
    TenantDeploymentMode Mode = TenantDeploymentMode.Multi,
    IReadOnlyList<string>? Roles = null,
    IReadOnlyList<string>? RoleSlugs = null,
    IReadOnlyList<string>? Permissions = null,
    Guid? EmployeeId = null,
    Guid? DepartmentId = null,
    IReadOnlyDictionary<string, string>? Attributes = null,
    IReadOnlyList<string>? Features = null,
    bool IsPlatformAdmin = false,
    bool IsResolved = false) : ITenantContext
{
    private static readonly IReadOnlyList<string> EmptyList = [];
    private static readonly IReadOnlyDictionary<string, string> EmptyAttributes =
        new Dictionary<string, string>();

    IReadOnlyList<string> ITenantContext.Roles => Roles ?? EmptyList;
    IReadOnlyList<string> ITenantContext.RoleSlugs => RoleSlugs ?? EmptyList;
    IReadOnlyList<string> ITenantContext.Permissions => Permissions ?? EmptyList;
    IReadOnlyDictionary<string, string> ITenantContext.Attributes => Attributes ?? EmptyAttributes;
    IReadOnlyList<string> ITenantContext.Features => Features ?? EmptyList;

    public bool HasPermission(string permission) =>
        (Permissions ?? EmptyList).Contains(permission, StringComparer.Ordinal);

    public static TenantContext Empty =>
        new(Guid.Empty, string.Empty, IsResolved: false);

    public static TenantContext CreateTenantOnly(
        Guid tenantId,
        string tenantSlug,
        TenantDeploymentMode mode = TenantDeploymentMode.Multi,
        IReadOnlyList<string>? features = null) =>
        new(
            tenantId,
            tenantSlug,
            Mode: mode,
            Features: features ?? EmptyList,
            IsResolved: true);

    public static TenantContext CreateSingleTenantDefault(
        Guid tenantId,
        string tenantSlug,
        IReadOnlyList<string>? features = null) =>
        CreateTenantOnly(tenantId, tenantSlug, TenantDeploymentMode.Single, features);
}
