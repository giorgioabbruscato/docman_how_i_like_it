namespace HrPortal.Employees.Application;

/// <summary>
/// Public contract for cross-module employee validation.
/// </summary>
public interface IEmployeeLookup
{
    Task<bool> ExistsAndIsActiveAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetActiveEmployeeIdsInDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<string?> GetFullNameAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, Guid?>> GetDepartmentIdsAsync(
        IReadOnlyList<Guid> employeeIds,
        CancellationToken cancellationToken = default);
    Task<int> CountActiveEmployeesAsync(
        IReadOnlyList<Guid>? restrictToEmployeeIds,
        CancellationToken cancellationToken = default);
}
