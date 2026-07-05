using HrPortal.Audit.Application;
using HrPortal.Tasks.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Tasks.Application.Commands;

public sealed class DeleteProjectTaskCommandHandler
{
    private readonly IProjectTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<DeleteProjectTaskCommandHandler> _logger;

    public DeleteProjectTaskCommandHandler(
        IProjectTaskRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<DeleteProjectTaskCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await _repository.GetByIdAsync(id, cancellationToken);
        if (task is null)
            return Result.Failure("Task not found.", "NOT_FOUND");

        await _repository.DeleteAsync(task, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "task.deleted",
            nameof(ProjectTask),
            task.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Task {TaskId} deleted", task.Id);
        return Result.Success();
    }
}
