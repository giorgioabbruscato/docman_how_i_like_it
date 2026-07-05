using HrPortal.SharedKernel.Results;
using HrPortal.Workflows.Application.Dtos;
using HrPortal.Workflows.Domain;

namespace HrPortal.Workflows.Application;

public interface IWorkflowCompletionHandler
{
    Domain.WorkflowRequestType RequestType { get; }

    Task<Result> HandleCompletionAsync(
        Domain.WorkflowInstance instance,
        Domain.WorkflowStatus finalStatus,
        Guid actorEmployeeId,
        string? comment,
        CancellationToken cancellationToken = default);
}

public interface IWorkflowEngine
{
    Task<Result<Dtos.WorkflowInstanceDto>> StartWorkflowAsync(
        Domain.WorkflowRequestType requestType,
        Guid requestId,
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<Result<Dtos.WorkflowInstanceDto>> ProcessActionAsync(
        Guid instanceId,
        Domain.WorkflowActionType action,
        Guid actorUserId,
        Guid? actorEmployeeId,
        string? comment,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<Dtos.PendingActionDto>>> GetPendingForActorAsync(
        Guid actorUserId,
        Guid? actorEmployeeId,
        CancellationToken cancellationToken = default);
}

public interface IWorkflowDefinitionService
{
    Task<Result<IReadOnlyList<WorkflowDefinitionDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<WorkflowDefinitionDto>> CreateAsync(
        CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default);
    Task<Result<WorkflowDefinitionDto>> UpdateAsync(
        Guid id,
        UpdateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default);
}

public interface IWorkflowQueryService
{
    Task<Result<IReadOnlyList<WorkflowInstanceDto>>> GetInstancesAsync(
        CancellationToken cancellationToken = default);
    Task<Result<WorkflowInstanceDto>> GetInstanceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
