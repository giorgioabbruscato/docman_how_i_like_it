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

public sealed class UpdateProjectTaskCommandHandler
{
    private readonly IProjectTaskRepository _repository;
    private readonly IProjectLookup _projectLookup;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<UpdateProjectTaskCommandHandler> _logger;

    public UpdateProjectTaskCommandHandler(
        IProjectTaskRepository repository,
        IProjectLookup projectLookup,
        IEmployeeLookup employeeLookup,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<UpdateProjectTaskCommandHandler> logger)
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
        Guid id,
        UpdateProjectTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result.Failure<ProjectTaskDto>("Task not found.", "NOT_FOUND");

        if (!await _projectLookup.ExistsAsync(request.ProjectId, cancellationToken))
            return Result.Failure<ProjectTaskDto>("Project not found.", "NOT_FOUND");

        if (request.AssignedEmployeeId.HasValue
            && !await _employeeLookup.ExistsAndIsActiveAsync(request.AssignedEmployeeId.Value, cancellationToken))
        {
            return Result.Failure<ProjectTaskDto>("Employee not found or inactive.", "NOT_FOUND");
        }

        try
        {
            task.Update(
                request.ProjectId,
                request.Title,
                request.Priority,
                request.Status,
                request.Description,
                request.AssignedEmployeeId,
                request.EstimatedHours,
                request.SpentHours,
                request.DueDate,
                _tenantContext.UserId);

            await _repository.UpdateAsync(task, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "task.updated",
                nameof(ProjectTask),
                task.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Task {TaskId} updated", task.Id);
            return Result.Success(TaskMapping.ToDto(task));
        }
        catch (DomainException ex)
        {
            return Result.Failure<ProjectTaskDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
