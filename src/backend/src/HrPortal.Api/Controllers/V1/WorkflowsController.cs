using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Tenancy;
using HrPortal.Workflows.Application;
using HrPortal.Workflows.Application.Dtos;
using HrPortal.Workflows.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Configurable approval workflows.</summary>
[ApiController]
[Route("api/v1/workflows")]
[Tags("Workflows")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class WorkflowsController : ControllerBase
{
    private readonly IWorkflowDefinitionService _definitionService;
    private readonly IWorkflowQueryService _queryService;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly TenantContext _tenantContext;

    public WorkflowsController(
        IWorkflowDefinitionService definitionService,
        IWorkflowQueryService queryService,
        IWorkflowEngine workflowEngine,
        TenantContext tenantContext)
    {
        _definitionService = definitionService;
        _queryService = queryService;
        _workflowEngine = workflowEngine;
        _tenantContext = tenantContext;
    }

    /// <summary>List workflow definitions.</summary>
    /// <remarks>Auth: workflow.manage:tenant</remarks>
    [HttpGet("definitions")]
    [RequirePermission(Permissions.WorkflowManageTenant)]
    [ProducesResponseType(typeof(IEnumerable<WorkflowDefinitionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDefinitions(CancellationToken cancellationToken)
    {
        var result = await _definitionService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a workflow definition.</summary>
    /// <remarks>Auth: workflow.manage:tenant</remarks>
    [HttpPost("definitions")]
    [RequirePermission(Permissions.WorkflowManageTenant)]
    [ProducesResponseType(typeof(WorkflowDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDefinition(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _definitionService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : MapFailure(result);
    }

    /// <summary>Update a workflow definition (creates new version).</summary>
    /// <remarks>Auth: workflow.manage:tenant</remarks>
    [HttpPut("definitions/{id:guid}")]
    [RequirePermission(Permissions.WorkflowManageTenant)]
    [ProducesResponseType(typeof(WorkflowDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDefinition(
        Guid id,
        [FromBody] UpdateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _definitionService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>List workflow instances.</summary>
    /// <remarks>Auth: workflow.read:team</remarks>
    [HttpGet("instances")]
    [RequirePermission(Permissions.WorkflowReadTeam)]
    [ProducesResponseType(typeof(IEnumerable<WorkflowInstanceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInstances(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetInstancesAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get workflow instance by ID.</summary>
    /// <remarks>Auth: workflow.read:team</remarks>
    [HttpGet("instances/{id:guid}")]
    [RequirePermission(Permissions.WorkflowReadTeam)]
    [ProducesResponseType(typeof(WorkflowInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInstance(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetInstanceByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Approve the current workflow step.</summary>
    /// <remarks>Auth: workflow.act:team</remarks>
    [HttpPost("instances/{id:guid}/approve")]
    [RequirePermission(Permissions.WorkflowActTeam)]
    [ProducesResponseType(typeof(WorkflowInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveInstance(
        Guid id,
        [FromBody] ProcessWorkflowActionRequest? request,
        CancellationToken cancellationToken)
    {
        var actor = ResolveActor();
        if (actor is null)
            return Forbidden("User context is required.");

        var result = await _workflowEngine.ProcessActionAsync(
            id,
            WorkflowActionType.Approve,
            actor.Value.UserId,
            actor.Value.EmployeeId,
            request?.Comment,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Reject the current workflow step.</summary>
    /// <remarks>Auth: workflow.act:team</remarks>
    [HttpPost("instances/{id:guid}/reject")]
    [RequirePermission(Permissions.WorkflowActTeam)]
    [ProducesResponseType(typeof(WorkflowInstanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectInstance(
        Guid id,
        [FromBody] ProcessWorkflowActionRequest? request,
        CancellationToken cancellationToken)
    {
        var actor = ResolveActor();
        if (actor is null)
            return Forbidden("User context is required.");

        var result = await _workflowEngine.ProcessActionAsync(
            id,
            WorkflowActionType.Reject,
            actor.Value.UserId,
            actor.Value.EmployeeId,
            request?.Comment,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>List pending workflow actions for the current user.</summary>
    /// <remarks>Auth: workflow.act:team</remarks>
    [HttpGet("pending")]
    [RequirePermission(Permissions.WorkflowActTeam)]
    [ProducesResponseType(typeof(IEnumerable<PendingActionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var actor = ResolveActor();
        if (actor is null)
            return Forbidden("User context is required.");

        var result = await _workflowEngine.GetPendingForActorAsync(
            actor.Value.UserId,
            actor.Value.EmployeeId,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    private (Guid UserId, Guid? EmployeeId)? ResolveActor()
    {
        if (!_tenantContext.UserId.HasValue)
            return null;

        return (_tenantContext.UserId.Value, _tenantContext.EmployeeId);
    }

    private IActionResult Forbidden(string detail) =>
        StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = detail
        });

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = result.Error
            }),
            "FORBIDDEN" => Forbidden(result.Error ?? "Forbidden"),
            "CONFLICT" => Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = result.Error
            }),
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = result.Error
            })
        };
}
