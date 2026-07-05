using HrPortal.Audit.Application;
using HrPortal.Departments.Application.Dtos;
using HrPortal.Departments.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Departments.Application;

public interface IDepartmentService : IDepartmentLookup
{
    Task<Result<IReadOnlyList<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<DepartmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<DepartmentDto>> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<Result<DepartmentDto>> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

internal sealed class DepartmentService : IDepartmentService, IDepartmentLookup
{
    private readonly IDepartmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<DepartmentService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<DepartmentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var departments = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(departments.Select(MapToDto).ToList() as IReadOnlyList<DepartmentDto>);
    }

    public async Task<Result<DepartmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, cancellationToken);
        if (department is null)
            return Result.Failure<DepartmentDto>("Department not found.", "NOT_FOUND");

        return Result.Success(MapToDto(department));
    }

    public async Task<Result<DepartmentDto>> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken: cancellationToken))
            return Result.Failure<DepartmentDto>("A department with this code already exists.", "CONFLICT");

        var parentValidation = await ValidateParentDepartmentAsync(request.ParentDepartmentId, null, cancellationToken);
        if (!parentValidation.IsSuccess)
            return Result.Failure<DepartmentDto>(parentValidation.Error!, parentValidation.ErrorCode);

        var department = Department.Create(
            _tenantContext.TenantId,
            request.Name,
            request.Code,
            request.Description,
            request.ParentDepartmentId,
            _tenantContext.UserId);

        await _repository.AddAsync(department, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "department.created",
            nameof(Department),
            department.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Department {DepartmentId} created", department.Id);
        return Result.Success(MapToDto(department));
    }

    public async Task<Result<DepartmentDto>> UpdateAsync(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, cancellationToken);
        if (department is null)
            return Result.Failure<DepartmentDto>("Department not found.", "NOT_FOUND");

        if (await _repository.CodeExistsAsync(request.Code, id, cancellationToken))
            return Result.Failure<DepartmentDto>("A department with this code already exists.", "CONFLICT");

        var parentValidation = await ValidateParentDepartmentAsync(request.ParentDepartmentId, id, cancellationToken);
        if (!parentValidation.IsSuccess)
            return Result.Failure<DepartmentDto>(parentValidation.Error!, parentValidation.ErrorCode);

        department.Update(
            request.Name,
            request.Code,
            request.Description,
            request.ParentDepartmentId,
            _tenantContext.UserId);

        await _repository.UpdateAsync(department, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "department.updated",
            nameof(Department),
            department.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(department));
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(id, cancellationToken);
        if (department is null)
            return Result.Failure("Department not found.", "NOT_FOUND");

        if (await _repository.HasActiveChildrenAsync(id, cancellationToken))
            return Result.Failure("Cannot deactivate a department with active child departments.", "CONFLICT");

        department.Deactivate(_tenantContext.UserId);
        await _repository.UpdateAsync(department, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "department.deactivated",
            nameof(Department),
            department.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<bool> ExistsAndIsActiveAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(departmentId, cancellationToken);
        return department is not null && department.IsActive;
    }

    public async Task<string?> GetNameAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        var department = await _repository.GetByIdAsync(departmentId, cancellationToken);
        return department?.Name;
    }

    private async Task<Result> ValidateParentDepartmentAsync(
        Guid? parentDepartmentId,
        Guid? currentDepartmentId,
        CancellationToken cancellationToken)
    {
        if (!parentDepartmentId.HasValue)
            return Result.Success();

        if (currentDepartmentId.HasValue && parentDepartmentId.Value == currentDepartmentId.Value)
            return Result.Failure("A department cannot be its own parent.", "VALIDATION_ERROR");

        var parent = await _repository.GetByIdAsync(parentDepartmentId.Value, cancellationToken);
        if (parent is null || !parent.IsActive)
            return Result.Failure("Parent department not found or inactive.", "NOT_FOUND");

        return Result.Success();
    }

    private static DepartmentDto MapToDto(Department department) =>
        new(
            department.Id,
            department.Name,
            department.Code,
            department.Description,
            department.ParentDepartmentId,
            department.IsActive);
}
