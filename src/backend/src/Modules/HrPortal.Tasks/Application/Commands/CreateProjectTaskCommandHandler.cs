using HrPortal.Audit.Application;
using HrPortal.Employees.Application;
using HrPortal.Projects.Application;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Tasks.Application.Commands;

public sealed class CreateProjectTaskCommandHandler
{
    private readonly IProjectTaskRepository _repository;
    private readonly IProjectLookup _projectLookup;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<CreateProjectTaskCommandHandler> _logger;

    public CreateProjectTaskCommandHandler(
        IProjectTaskRepository repository,
        IProjectLookup projectLookup,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<CreateProjectTaskCommandHandler> logger)
    {
        _repository = repository;
        _projectLookup = projectLookup;
        _employeeLookup = employeeLookup;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<ProjectTaskDto>> HandleAsync(
        CreateProjectTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!await _projectLookup.ExistsAsync(request.ProjectId, cancellationToken))
            return Result.Failure<ProjectTaskDto>("Project not found.", "NOT_FOUND");

        if (request.AssignedEmployeeId.HasValue
            && !await _employeeLookup.ExistsAndIsActiveAsync(request.AssignedEmployeeId.Value, cancellationToken))
        {
            return Result.Failure<ProjectTaskDto>("Employee not found or inactive.", "NOT_FOUND");
        }

        try
        {
            var task = ProjectTask.Create(
                _tenantContext.TenantId,
                request.ProjectId,
                request.Title,
                request.Priority,
                request.Status,
                request.Description,
                request.AssignedEmployeeId,
                request.EstimatedHours,
                request.DueDate,
                _tenantContext.UserId);

            await _repository.AddAsync(task, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "task.created",
                nameof(ProjectTask),
                task.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} created", task.Id);
            return Result.Success(TaskMapping.ToDto(task));
        }
        catch (DomainException ex)
        {
            return Result.Failure<ProjectTaskDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
