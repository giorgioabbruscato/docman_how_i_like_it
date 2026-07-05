namespace HrPortal.Workflows.Application.Dtos;

public sealed record WorkflowDefinitionDto(
    Guid Id,
    string RequestType,
    string Name,
    string StepsJson,
    bool IsActive,
    int Version);

public sealed record CreateWorkflowDefinitionRequest(
    string RequestType,
    string Name,
    string StepsJson);

public sealed record UpdateWorkflowDefinitionRequest(
    string Name,
    string StepsJson);

public sealed record WorkflowInstanceDto(
    Guid Id,
    Guid WorkflowDefinitionId,
    string RequestType,
    Guid RequestId,
    Guid EmployeeId,
    string Status,
    int CurrentStepIndex,
    string? CurrentStepName,
    DateTime StartedAt,
    DateTime? CompletedAt,
    IReadOnlyList<WorkflowActionDto> Actions);

public sealed record WorkflowActionDto(
    Guid Id,
    int StepIndex,
    Guid ActorEmployeeId,
    string Action,
    string? Comment,
    DateTime ActionAt);

public sealed record PendingActionDto(
    Guid InstanceId,
    string RequestType,
    Guid RequestId,
    Guid RequesterEmployeeId,
    int StepIndex,
    string StepName,
    DateTime StartedAt);

public sealed record ProcessWorkflowActionRequest(string? Comment);
