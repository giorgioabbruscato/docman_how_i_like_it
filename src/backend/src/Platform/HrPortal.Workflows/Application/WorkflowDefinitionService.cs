using HrPortal.SharedKernel.Exceptions;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;
using HrPortal.Workflows.Application.Dtos;
using HrPortal.Workflows.Domain;

namespace HrPortal.Workflows.Application;

internal sealed class WorkflowDefinitionService : IWorkflowDefinitionService
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;

    public WorkflowDefinitionService(
        IWorkflowDefinitionRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<WorkflowDefinitionDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var definitions = await _repository.GetAllAsync(cancellationToken);
        return Result.Success(definitions.Select(MapToDto).ToList() as IReadOnlyList<WorkflowDefinitionDto>);
    }

    public async Task<Result<WorkflowDefinitionDto>> CreateAsync(
        CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<WorkflowRequestType>(request.RequestType, true, out var requestType))
            return Result.Failure<WorkflowDefinitionDto>("Invalid workflow request type.", "VALIDATION_ERROR");

        try
        {
            WorkflowDefinition.ValidateStepsJson(request.StepsJson);
        }
        catch (DomainException ex)
        {
            return Result.Failure<WorkflowDefinitionDto>(ex.Message, "VALIDATION_ERROR");
        }

        var existing = await _repository.GetActiveByRequestTypeAsync(requestType, cancellationToken);
        if (existing is not null)
        {
            existing.Deactivate(_tenantContext.UserId);
            await _repository.UpdateAsync(existing, cancellationToken);
        }

        var definition = WorkflowDefinition.Create(
            _tenantContext.TenantId,
            requestType,
            request.Name,
            request.StepsJson,
            version: existing?.Version + 1 ?? 1,
            _tenantContext.UserId);

        await _repository.AddAsync(definition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(definition));
    }

    public async Task<Result<WorkflowDefinitionDto>> UpdateAsync(
        Guid id,
        UpdateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return Result.Failure<WorkflowDefinitionDto>("Workflow definition not found.", "NOT_FOUND");

        try
        {
            WorkflowDefinition.ValidateStepsJson(request.StepsJson);
        }
        catch (DomainException ex)
        {
            return Result.Failure<WorkflowDefinitionDto>(ex.Message, "VALIDATION_ERROR");
        }

        existing.Deactivate(_tenantContext.UserId);
        await _repository.UpdateAsync(existing, cancellationToken);

        var newVersion = existing.CreateNewVersion(request.Name, request.StepsJson, _tenantContext.UserId);
        await _repository.AddAsync(newVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(MapToDto(newVersion));
    }

    private static WorkflowDefinitionDto MapToDto(WorkflowDefinition definition) =>
        new(
            definition.Id,
            definition.RequestType.ToString(),
            definition.Name,
            definition.StepsJson,
            definition.IsActive,
            definition.Version);
}

internal sealed class WorkflowQueryService : IWorkflowQueryService
{
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IWorkflowActionRepository _actionRepository;

    public WorkflowQueryService(
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowActionRepository actionRepository)
    {
        _instanceRepository = instanceRepository;
        _definitionRepository = definitionRepository;
        _actionRepository = actionRepository;
    }

    public async Task<Result<IReadOnlyList<WorkflowInstanceDto>>> GetInstancesAsync(
        CancellationToken cancellationToken = default)
    {
        var instances = await _instanceRepository.GetAllAsync(cancellationToken);
        var dtos = new List<WorkflowInstanceDto>();

        foreach (var instance in instances)
        {
            var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, cancellationToken);
            if (definition is null)
                continue;

            dtos.Add(await MapInstanceAsync(instance, definition, cancellationToken));
        }

        return Result.Success<IReadOnlyList<WorkflowInstanceDto>>(dtos);
    }

    public async Task<Result<WorkflowInstanceDto>> GetInstanceByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var instance = await _instanceRepository.GetByIdAsync(id, cancellationToken);
        if (instance is null)
            return Result.Failure<WorkflowInstanceDto>("Workflow instance not found.", "NOT_FOUND");

        var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, cancellationToken);
        if (definition is null)
            return Result.Failure<WorkflowInstanceDto>("Workflow definition not found.", "NOT_FOUND");

        return Result.Success(await MapInstanceAsync(instance, definition, cancellationToken));
    }

    private async Task<WorkflowInstanceDto> MapInstanceAsync(
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
