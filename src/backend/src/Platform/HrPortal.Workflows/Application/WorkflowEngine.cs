using HrPortal.Audit.Application;
using HrPortal.Notifications;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using HrPortal.Workflows.Application.Dtos;
using HrPortal.Workflows.Domain;
using Microsoft.Extensions.Logging;

namespace HrPortal.Workflows.Application;

internal sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowActionRepository _actionRepository;
    private readonly IWorkflowApproverResolver _approverResolver;
    private readonly IEnumerable<IWorkflowCompletionHandler> _completionHandlers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowActionRepository actionRepository,
        IWorkflowApproverResolver approverResolver,
        IEnumerable<IWorkflowCompletionHandler> completionHandlers,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        IAuditService auditService,
        INotificationService notificationService,
        ILogger<WorkflowEngine> logger)
    {
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _actionRepository = actionRepository;
        _approverResolver = approverResolver;
        _completionHandlers = completionHandlers;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _auditService = auditService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<WorkflowInstanceDto>> StartWorkflowAsync(
        WorkflowRequestType requestType,
        Guid requestId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var definition = await _definitionRepository.GetActiveByRequestTypeAsync(requestType, cancellationToken);
        if (definition is null)
            return Result.Failure<WorkflowInstanceDto>(
                $"No active workflow definition found for {requestType}.",
                "NOT_FOUND");

        var existing = await _instanceRepository.GetActiveByRequestAsync(requestType, requestId, cancellationToken);
        if (existing is not null)
            return Result.Failure<WorkflowInstanceDto>("Workflow instance already exists for this request.", "CONFLICT");

        var instance = WorkflowInstance.Create(
            _tenantContext.TenantId,
            definition.Id,
            requestType,
            requestId,
            employeeId,
            _tenantContext.UserId);

        await _instanceRepository.AddAsync(instance, cancellationToken);
        await _auditService.LogAsync(new AuditEntry(
            "workflow.started",
            nameof(WorkflowInstance),
            instance.Id.ToString()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await NotifyCurrentApproversAsync(definition, instance, cancellationToken);

        _logger.LogInformation(
            "Workflow {InstanceId} started for {RequestType} request {RequestId}",
            instance.Id,
            requestType,
            requestId);

        return Result.Success(await MapInstanceAsync(instance, definition, cancellationToken));
    }

    public async Task<Result<WorkflowInstanceDto>> ProcessActionAsync(
        Guid instanceId,
        WorkflowActionType action,
        Guid actorUserId,
        Guid? actorEmployeeId,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepository.GetByIdAsync(instanceId, cancellationToken);
        if (instance is null)
            return Result.Failure<WorkflowInstanceDto>("Workflow instance not found.", "NOT_FOUND");

        if (instance.IsTerminal)
            return Result.Failure<WorkflowInstanceDto>("Workflow is already completed.", "CONFLICT");

        var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, cancellationToken);
        if (definition is null)
            return Result.Failure<WorkflowInstanceDto>("Workflow definition not found.", "NOT_FOUND");

        var steps = definition.ParseSteps().Steps;
        var recordedActorEmployeeId = actorEmployeeId ?? instance.EmployeeId;

        if (action is WorkflowActionType.Approve or WorkflowActionType.Reject)
        {
            if (instance.CurrentStepIndex >= steps.Count)
                return Result.Failure<WorkflowInstanceDto>("Workflow has no pending step.", "CONFLICT");

            var currentStep = steps[instance.CurrentStepIndex];
            var approvers = await _approverResolver.ResolveApproversAsync(
                currentStep,
                instance.EmployeeId,
                cancellationToken);

            if (!IsAuthorizedActor(approvers, actorUserId, actorEmployeeId))
                return Result.Failure<WorkflowInstanceDto>(
                    "You are not authorized to act on this workflow step.",
                    "FORBIDDEN");
        }
        else if (action == WorkflowActionType.Cancel)
        {
            if (actorEmployeeId != instance.EmployeeId && actorUserId != _tenantContext.UserId)
                return Result.Failure<WorkflowInstanceDto>(
                    "Only the requester can cancel the workflow.",
                    "FORBIDDEN");
        }

        var workflowAction = WorkflowAction.Record(
            _tenantContext.TenantId,
            instance.Id,
            instance.CurrentStepIndex,
            recordedActorEmployeeId,
            action,
            comment);

        await _actionRepository.AddAsync(workflowAction, cancellationToken);

        switch (action)
        {
            case WorkflowActionType.Approve:
                if (instance.CurrentStepIndex >= steps.Count - 1)
                {
                    instance.MarkApproved(_tenantContext.UserId);
                    var completion = await InvokeCompletionHandlerAsync(
                        instance,
                        WorkflowStatus.Approved,
                        recordedActorEmployeeId,
                        comment,
                        cancellationToken);
                    if (!completion.IsSuccess)
                        return Result.Failure<WorkflowInstanceDto>(completion.Error!, completion.ErrorCode);
                }
                else
                {
                    instance.AdvanceStep(_tenantContext.UserId);
                    await NotifyCurrentApproversAsync(definition, instance, cancellationToken);
                }
                break;

            case WorkflowActionType.Reject:
                instance.MarkRejected(_tenantContext.UserId);
                var rejectCompletion = await InvokeCompletionHandlerAsync(
                    instance,
                    WorkflowStatus.Rejected,
                    recordedActorEmployeeId,
                    comment,
                    cancellationToken);
                if (!rejectCompletion.IsSuccess)
                    return Result.Failure<WorkflowInstanceDto>(rejectCompletion.Error!, rejectCompletion.ErrorCode);
                break;

            case WorkflowActionType.Cancel:
                instance.MarkCancelled(_tenantContext.UserId);
                var cancelCompletion = await InvokeCompletionHandlerAsync(
                    instance,
                    WorkflowStatus.Cancelled,
                    recordedActorEmployeeId,
                    comment,
                    cancellationToken);
                if (!cancelCompletion.IsSuccess)
                    return Result.Failure<WorkflowInstanceDto>(cancelCompletion.Error!, cancelCompletion.ErrorCode);
                break;

            default:
                return Result.Failure<WorkflowInstanceDto>("Unsupported workflow action.", "VALIDATION_ERROR");
        }

        await _instanceRepository.UpdateAsync(instance, cancellationToken);
        await _auditService.LogAsync(new AuditEntry(
            $"workflow.{action.ToString().ToLowerInvariant()}",
            nameof(WorkflowInstance),
            instance.Id.ToString()), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(await MapInstanceAsync(instance, definition, cancellationToken));
    }

    public async Task<Result<IReadOnlyList<PendingActionDto>>> GetPendingForActorAsync(
        Guid actorUserId,
        Guid? actorEmployeeId,
        CancellationToken cancellationToken = default)
    {
        var instances = await _instanceRepository.GetActiveInProgressAsync(cancellationToken);
        var pending = new List<PendingActionDto>();

        foreach (var instance in instances)
        {
            var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, cancellationToken);
            if (definition is null)
                continue;

            var steps = definition.ParseSteps().Steps;
            if (instance.CurrentStepIndex >= steps.Count)
                continue;

            var currentStep = steps[instance.CurrentStepIndex];
            var approvers = await _approverResolver.ResolveApproversAsync(
                currentStep,
                instance.EmployeeId,
                cancellationToken);

            if (!IsAuthorizedActor(approvers, actorUserId, actorEmployeeId))
                continue;

            pending.Add(new PendingActionDto(
                instance.Id,
                instance.RequestType.ToString(),
                instance.RequestId,
                instance.EmployeeId,
                instance.CurrentStepIndex,
                currentStep.Name,
                instance.StartedAt));
        }

        return Result.Success<IReadOnlyList<PendingActionDto>>(pending);
    }

    private static bool IsAuthorizedActor(
        IReadOnlyList<WorkflowApprover> approvers,
        Guid actorUserId,
        Guid? actorEmployeeId) =>
        approvers.Any(a =>
            a.UserId == actorUserId
            || (actorEmployeeId.HasValue && a.EmployeeId == actorEmployeeId));

    private async Task NotifyCurrentApproversAsync(
        WorkflowDefinition definition,
        WorkflowInstance instance,
        CancellationToken cancellationToken)
    {
        var steps = definition.ParseSteps().Steps;
        if (instance.CurrentStepIndex >= steps.Count)
            return;

        var currentStep = steps[instance.CurrentStepIndex];
        var approvers = await _approverResolver.ResolveApproversAsync(
            currentStep,
            instance.EmployeeId,
            cancellationToken);

        foreach (var approver in approvers)
        {
            var employeeId = approver.EmployeeId ?? instance.EmployeeId;
            await NotificationHelper.TryNotifyAsync(
                _logger,
                ct => _notificationService.NotifyWorkflowActionRequiredAsync(
                    employeeId,
                    instance.RequestType.ToString(),
                    instance.RequestId,
                    currentStep.Name,
                    ct),
                cancellationToken);
        }
    }

    private async Task<Result> InvokeCompletionHandlerAsync(
        WorkflowInstance instance,
        WorkflowStatus finalStatus,
        Guid actorEmployeeId,
        string? comment,
        CancellationToken cancellationToken)
    {
        var handler = _completionHandlers.FirstOrDefault(h => h.RequestType == instance.RequestType);
        if (handler is null)
            return Result.Success();

        return await handler.HandleCompletionAsync(instance, finalStatus, actorEmployeeId, comment, cancellationToken);
    }

    internal async Task<WorkflowInstanceDto> MapInstanceAsync(
        WorkflowInstance instance,
        WorkflowDefinition definition,
        CancellationToken cancellationToken)
    {
        var steps = definition.ParseSteps().Steps;
        string? currentStepName = instance.CurrentStepIndex < steps.Count
            ? steps[instance.CurrentStepIndex].Name
            : null;

        var actions = await _actionRepository.GetByInstanceIdAsync(instance.Id, cancellationToken);

        return new WorkflowInstanceDto(
            instance.Id,
            instance.WorkflowDefinitionId,
            instance.RequestType.ToString(),
            instance.RequestId,
            instance.EmployeeId,
            instance.Status.ToString(),
            instance.CurrentStepIndex,
            currentStepName,
            instance.StartedAt,
            instance.CompletedAt,
            actions.Select(a => new WorkflowActionDto(
                a.Id,
                a.StepIndex,
                a.ActorEmployeeId,
                a.Action.ToString(),
                a.Comment,
                a.ActionAt)).ToList());
    }
}
