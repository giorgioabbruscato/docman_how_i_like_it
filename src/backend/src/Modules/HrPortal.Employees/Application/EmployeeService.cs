using HrPortal.Audit.Application;
using HrPortal.Departments.Application;
using HrPortal.Employees.Application.Dtos;
using HrPortal.Employees.Domain;
using HrPortal.Identity;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Employees.Application;

public interface IEmployeeService : IEmployeeLookup
{
    Task<Result<IReadOnlyList<EmployeeDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<EmployeeDto>> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<Result<EmployeeDto>> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

internal sealed class EmployeeService : IEmployeeService, IEmployeeLookup
{
    private readonly IEmployeeRepository _repository;
    private readonly IDepartmentLookup _departmentLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly UserContext _userContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository repository,
        IDepartmentLookup departmentLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        UserContext userContext,
        IAuditService auditService,
        ILogger<EmployeeService> logger)
    {
        _repository = repository;
        _departmentLookup = departmentLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _userContext = userContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<EmployeeDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(employees.Select(MapToDto).ToList() as IReadOnlyList<EmployeeDto>);
    }

    public async Task<Result<EmployeeDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeDto>("Employee not found.", "NOT_FOUND");

        return Result.Success(MapToDto(employee));
    }

    public async Task<Result<EmployeeDto>> CreateAsync(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantResolved();

        if (await _repository.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
            return Result.Failure<EmployeeDto>("An employee with this email already exists.", "CONFLICT");

        var departmentValidation = await ValidateDepartmentAsync(request.DepartmentId, cancellationToken);
        if (!departmentValidation.IsSuccess)
            return Result.Failure<EmployeeDto>(departmentValidation.Error!, departmentValidation.ErrorCode);

        var employee = Employee.Create(
            _tenantContext.TenantId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.HireDate,
            request.JobTitle,
            request.DepartmentId,
            _userContext.UserId);

        await _repository.AddAsync(employee, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "employee.created",
            nameof(Employee),
            employee.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} created", employee.Id);
        return Result.Success(MapToDto(employee));
    }

    public async Task<Result<EmployeeDto>> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
            return Result.Failure<EmployeeDto>("Employee not found.", "NOT_FOUND");

        if (await _repository.EmailExistsAsync(request.Email, id, cancellationToken))
            return Result.Failure<EmployeeDto>("An employee with this email already exists.", "CONFLICT");

        var departmentValidation = await ValidateDepartmentAsync(request.DepartmentId, cancellationToken);
        if (!departmentValidation.IsSuccess)
            return Result.Failure<EmployeeDto>(departmentValidation.Error!, departmentValidation.ErrorCode);

        employee.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.JobTitle,
            request.DepartmentId,
            _userContext.UserId);

        await _repository.UpdateAsync(employee, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "employee.updated",
            nameof(Employee),
            employee.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} updated", employee.Id);
        return Result.Success(MapToDto(employee));
    }

    public async Task<Result> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(id, cancellationToken);
        if (employee is null)
            return Result.Failure("Employee not found.", "NOT_FOUND");

        employee.Deactivate(_userContext.UserId);
        await _repository.UpdateAsync(employee, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "employee.deactivated",
            nameof(Employee),
            employee.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Employee {EmployeeId} deactivated", employee.Id);
        return Result.Success();
    }

    public async Task<bool> ExistsAndIsActiveAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _repository.GetByIdAsync(employeeId, cancellationToken);
        return employee is not null && employee.IsActive;
    }

    private async Task<Result> ValidateDepartmentAsync(
        Guid? departmentId,
        CancellationToken cancellationToken)
    {
        if (!departmentId.HasValue)
            return Result.Success();

        if (!await _departmentLookup.ExistsAndIsActiveAsync(departmentId.Value, cancellationToken))
            return Result.Failure("Department not found or inactive.", "NOT_FOUND");

        return Result.Success();
    }

    private void EnsureTenantResolved()
    {
        if (!_tenantContext.IsResolved)
            throw new DomainException("Tenant context is not resolved.");
    }

    private static EmployeeDto MapToDto(Employee employee) =>
        new(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Email,
            employee.JobTitle,
            employee.DepartmentId,
            employee.HireDate,
            employee.IsActive);
}
