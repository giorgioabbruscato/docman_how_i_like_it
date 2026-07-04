using HrPortal.SharedKernel.Entities;

namespace HrPortal.Departments.Domain;

public sealed class Department : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentDepartmentId { get; private set; }
    public bool IsActive { get; private set; }

    private Department() { }

    public static Department Create(
        Guid tenantId,
        string name,
        string code,
        string? description = null,
        Guid? parentDepartmentId = null,
        Guid? createdBy = null)
    {
        return new Department
        {
            Name = name,
            Code = code.ToUpperInvariant(),
            Description = description,
            ParentDepartmentId = parentDepartmentId,
            IsActive = true,
            CreatedBy = createdBy
        }.Also(d => d.SetTenant(tenantId));
    }

    public void Update(
        string name,
        string code,
        string? description,
        Guid? parentDepartmentId,
        Guid? updatedBy)
    {
        Name = name;
        Code = code.ToUpperInvariant();
        Description = description;
        ParentDepartmentId = parentDepartmentId;
        MarkUpdated(updatedBy);
    }

    public void Deactivate(Guid? updatedBy)
    {
        IsActive = false;
        MarkUpdated(updatedBy);
    }
}

internal static class DepartmentExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
