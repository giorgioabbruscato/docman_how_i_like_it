using HrPortal.Employees.Domain;

namespace HrPortal.Employees.Application;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetActiveEmployeeIdsInDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, Guid?>> GetDepartmentIdsByEmployeeIdsAsync(
        IReadOnlyList<Guid> employeeIds,
        CancellationToken cancellationToken = default);
    Task<int> CountActiveEmployeesAsync(
        IReadOnlyList<Guid>? restrictToEmployeeIds,
        CancellationToken cancellationToken = default);
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
}
