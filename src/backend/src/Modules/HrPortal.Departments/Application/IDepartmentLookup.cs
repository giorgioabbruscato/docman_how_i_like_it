namespace HrPortal.Departments.Application;

/// <summary>
/// Public contract for cross-module department validation.
/// </summary>
public interface IDepartmentLookup
{
    Task<bool> ExistsAndIsActiveAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<string?> GetNameAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
