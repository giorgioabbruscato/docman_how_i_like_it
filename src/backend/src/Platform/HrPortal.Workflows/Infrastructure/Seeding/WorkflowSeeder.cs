using HrPortal.Workflows.Application;
using HrPortal.Workflows.Domain;
using HrPortal.SharedKernel.Persistence;

namespace HrPortal.Workflows.Infrastructure.Seeding;

public interface IWorkflowSeeder
{
    Task SeedDefaultsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

internal sealed class WorkflowSeeder : IWorkflowSeeder
{
    private const string DefaultLeaveWorkflowSteps =
        """{"steps":[{"name":"Direct Manager","approverType":"DirectManager"}]}""";

    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WorkflowSeeder(IWorkflowDefinitionRepository definitionRepository, IUnitOfWork unitOfWork)
    {
        _definitionRepository = definitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SeedDefaultsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var existing = await _definitionRepository.GetActiveByRequestTypeAsync(
            WorkflowRequestType.Leave,
            cancellationToken);

        if (existing is not null)
            return;

        var definition = WorkflowDefinition.Create(
            tenantId,
            WorkflowRequestType.Leave,
            "Leave Approval",
            DefaultLeaveWorkflowSteps);

        await _definitionRepository.AddAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
