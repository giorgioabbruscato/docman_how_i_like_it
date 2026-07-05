using System.Text.Json;
using HrPortal.SharedKernel.Entities;

namespace HrPortal.AccessControl.Domain;

public sealed class TenantMembership : AuditableEntity
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public Guid UserId { get; private set; }
    public string RoleIdsJson { get; private set; } = "[]";
    public Guid? EmployeeId { get; private set; }
    public string? AttributesJson { get; private set; }
    public bool IsActive { get; private set; }

    private TenantMembership() { }

    public static TenantMembership Create(
        Guid tenantId,
        Guid userId,
        IReadOnlyList<Guid> roleIds,
        Guid? employeeId = null,
        IReadOnlyDictionary<string, string>? attributes = null,
        Guid? createdBy = null)
    {
        var membership = new TenantMembership
        {
            UserId = userId,
            EmployeeId = employeeId,
            IsActive = true,
            CreatedBy = createdBy
        };
        membership.SetTenant(tenantId);
        membership.SetRoleIds(roleIds);
        membership.SetAttributes(attributes);
        return membership;
    }

    public IReadOnlyList<Guid> GetRoleIds() =>
        JsonSerializer.Deserialize<List<Guid>>(RoleIdsJson, JsonOptions) ?? [];

    public void SetRoleIds(IReadOnlyList<Guid> roleIds) =>
        RoleIdsJson = JsonSerializer.Serialize(roleIds.Distinct().ToList(), JsonOptions);

    public IReadOnlyDictionary<string, string> GetAttributes()
    {
        if (string.IsNullOrWhiteSpace(AttributesJson))
            return new Dictionary<string, string>();

        return JsonSerializer.Deserialize<Dictionary<string, string>>(AttributesJson, JsonOptions)
            ?? new Dictionary<string, string>();
    }

    public void SetAttributes(IReadOnlyDictionary<string, string>? attributes) =>
        AttributesJson = attributes is null || attributes.Count == 0
            ? null
            : JsonSerializer.Serialize(attributes, JsonOptions);

    public void UpdateRoles(
        IReadOnlyList<Guid> roleIds,
        Guid? employeeId,
        IReadOnlyDictionary<string, string>? attributes,
        Guid? updatedBy)
    {
        SetRoleIds(roleIds);
        EmployeeId = employeeId;
        SetAttributes(attributes);
        MarkUpdated(updatedBy);
    }

    public void Deactivate(Guid? updatedBy)
    {
        IsActive = false;
        MarkUpdated(updatedBy);
    }
}
