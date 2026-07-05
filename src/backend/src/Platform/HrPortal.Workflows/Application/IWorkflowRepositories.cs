namespace HrPortal.Workflows.Application;

public interface IWorkflowDefinitionRepository
{
    Task<Domain.WorkflowDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.WorkflowDefinition?> GetActiveByRequestTypeAsync(
        Domain.WorkflowRequestType requestType,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WorkflowDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Domain.WorkflowDefinition definition, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.WorkflowDefinition definition, CancellationToken cancellationToken = default);
}

public interface IWorkflowInstanceRepository
{
    Task<Domain.WorkflowInstance?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.WorkflowInstance?> GetActiveByRequestAsync(
        Domain.WorkflowRequestType requestType,
        Guid requestId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WorkflowInstance>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WorkflowInstance>> GetActiveInProgressAsync(
        CancellationToken cancellationToken = default);
    Task AddAsync(Domain.WorkflowInstance instance, CancellationToken cancellationToken = default);
    Task UpdateAsync(Domain.WorkflowInstance instance, CancellationToken cancellationToken = default);
}

public interface IWorkflowActionRepository
{
    Task<IReadOnlyList<Domain.WorkflowAction>> GetByInstanceIdAsync(
        Guid instanceId,
        CancellationToken cancellationToken = default);
    Task AddAsync(Domain.WorkflowAction action, CancellationToken cancellationToken = default);
}
