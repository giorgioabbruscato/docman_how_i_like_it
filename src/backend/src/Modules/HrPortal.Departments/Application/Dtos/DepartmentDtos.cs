namespace HrPortal.Departments.Application.Dtos;

public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    Guid? ParentDepartmentId,
    bool IsActive);

public sealed record CreateDepartmentRequest(
    string Name,
    string Code,
    string? Description = null,
    Guid? ParentDepartmentId = null);

public sealed record UpdateDepartmentRequest(
    string Name,
    string Code,
    string? Description = null,
    Guid? ParentDepartmentId = null);
