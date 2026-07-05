using System.Text.Json;
using HrPortal.SharedKernel.Entities;

namespace HrPortal.AccessControl.Domain;

public sealed class TenantRole : AuditableEntity
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public string Slug { get; private set; } = string.Empty;
    public string PermissionsJson { get; private set; } = "[]";
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }

    private TenantRole() { }

    public static TenantRole Create(
        Guid tenantId,
        string slug,
        IReadOnlyList<string> permissions,
        bool isSystem,
        Guid? createdBy = null)
    {
        var role = new TenantRole
        {
            Slug = NormalizeSlug(slug),
            IsSystem = isSystem,
            IsActive = true,
            CreatedBy = createdBy
        };
        role.SetTenant(tenantId);
        role.SetPermissions(permissions);
        return role;
    }

    public IReadOnlyList<string> GetPermissions() =>
        JsonSerializer.Deserialize<List<string>>(PermissionsJson, JsonOptions) ?? [];

    public void SetPermissions(IReadOnlyList<string> permissions) =>
        PermissionsJson = JsonSerializer.Serialize(permissions.Distinct(StringComparer.Ordinal).ToList(), JsonOptions);

    public void UpdatePermissions(IReadOnlyList<string> permissions, Guid? updatedBy)
    {
        SetPermissions(permissions);
        MarkUpdated(updatedBy);
    }

    public void Deactivate(Guid? updatedBy)
    {
        IsActive = false;
        MarkUpdated(updatedBy);
    }

    public static string NormalizeSlug(string slug) =>
        slug.Trim().ToLowerInvariant();
}
