using System.Text.Json;
using HrPortal.Audit.Application;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Tasks.Application.Commands;

public sealed class UpdateTaskStatusCommandHandler
{
    private readonly IProjectTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<UpdateTaskStatusCommandHandler> _logger;

    public UpdateTaskStatusCommandHandler(
        IProjectTaskRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<UpdateTaskStatusCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<ProjectTaskDto>> HandleAsync(
        Guid id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result.Failure<ProjectTaskDto>("Task not found.", "NOT_FOUND");

        if (request.UpdatedAt.HasValue && task.UpdatedAt != request.UpdatedAt)
        {
            return Result.Failure<ProjectTaskDto>(
                "Task was modified by another user.",
                "CONFLICT");
        }

        var oldStatus = task.Status;

        try
        {
            task.UpdateStatus(request.Status, _tenantContext.UserId);

            await _repository.UpdateAsync(task, cancellationToken);

            var metadata = JsonSerializer.Serialize(new
            {
                oldStatus = oldStatus.ToString(),
                newStatus = request.Status.ToString()
            });

            await _auditService.LogAsync(new AuditEntry(
                "task.status_changed",
                nameof(ProjectTask),
                task.Id.ToString(),
                metadata), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Task {TaskId} status changed from {OldStatus} to {NewStatus}",
                task.Id,
                oldStatus,
                request.Status);

            return Result.Success(TaskMapping.ToDto(task));
        }
        catch (DomainException ex)
        {
            return Result.Failure<ProjectTaskDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
