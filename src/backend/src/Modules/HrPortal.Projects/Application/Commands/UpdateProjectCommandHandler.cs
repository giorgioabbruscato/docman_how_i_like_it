using HrPortal.Audit.Application;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;
using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Projects.Application.Commands;

public sealed class UpdateProjectCommandHandler
{
    private readonly IProjectRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly ILogger<UpdateProjectCommandHandler> _logger;

    public UpdateProjectCommandHandler(
        IProjectRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        ILogger<UpdateProjectCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<Result<ProjectDto>> HandleAsync(
        Guid id,
        UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _repository.GetByIdAsync(id, cancellationToken);
        if (project is null)
            return Result.Failure<ProjectDto>("Project not found.", "NOT_FOUND");

        try
        {
            project.Update(
                request.Name,
                request.Status,
                request.Description,
                request.CustomerName,
                request.StartDate,
                request.EndDate,
                request.BudgetHours,
                request.BudgetCost,
                _tenantContext.UserId);

            await _repository.UpdateAsync(project, cancellationToken);

            await _auditService.LogAsync(new AuditEntry(
                "project.updated",
                nameof(Project),
                project.Id.ToString()), cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project {ProjectId} updated", project.Id);
            return Result.Success(ProjectMapping.ToDto(project));
        }
        catch (DomainException ex)
        {
            return Result.Failure<ProjectDto>(ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR");
        }
    }
}
