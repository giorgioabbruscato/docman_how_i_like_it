namespace HrPortal.Employees.Application.Dtos;

public sealed record EmployeeDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle,
    Guid? DepartmentId,
    DateOnly HireDate,
    bool IsActive);

public sealed record CreateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    DateOnly HireDate,
    string? JobTitle = null,
    Guid? DepartmentId = null);

public sealed record UpdateEmployeeRequest(
    string FirstName,
    string LastName,
    string Email,
    string? JobTitle = null,
    Guid? DepartmentId = null);
