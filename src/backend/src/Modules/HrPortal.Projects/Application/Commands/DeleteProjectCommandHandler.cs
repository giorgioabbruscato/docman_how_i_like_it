using HrPortal.Audit.Application;
using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Projects.Application.Commands;

public sealed class DeleteProjectCommandHandler
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<DeleteProjectCommandHandler> _logger;

    public DeleteProjectCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<DeleteProjectCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result.Failure("Project not found.", "NOT_FOUND");

        project.Archive(_tenantContext.UserId);
        await _repository.UpdateAsync(project, cancellationToken);

        await _auditService.LogAsync(new AuditEntry(
            "project.deleted",
            nameof(Project),
            project.Id.ToString()), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} archived", project.Id);
        return Result.Success();
    }
}
