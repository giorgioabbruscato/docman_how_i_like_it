using HrPortal.SharedKernel.Entities;

namespace HrPortal.Employees.Domain;

public sealed class Employee : AuditableEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? JobTitle { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public DateOnly HireDate { get; private set; }
    public bool IsActive { get; private set; }

    private Employee() { }

    public static Employee Create(
        Guid tenantId,
        string firstName,
        string lastName,
        string email,
        DateOnly hireDate,
        string? jobTitle = null,
        Guid? departmentId = null,
        Guid? createdBy = null)
    {
        return new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            HireDate = hireDate,
            JobTitle = jobTitle,
            DepartmentId = departmentId,
            IsActive = true,
            CreatedBy = createdBy
        }.Also(e => e.SetTenant(tenantId));
    }

    public string FullName => $"{FirstName} {LastName}";

    public void Update(
        string firstName,
        string lastName,
        string email,
        string? jobTitle,
        Guid? departmentId,
        Guid? updatedBy)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email.ToLowerInvariant();
        JobTitle = jobTitle;
        DepartmentId = departmentId;
        MarkUpdated(updatedBy);
    }

    public void Deactivate(Guid? updatedBy)
    {
        IsActive = false;
        MarkUpdated(updatedBy);
    }
}

internal static class EmployeeExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
