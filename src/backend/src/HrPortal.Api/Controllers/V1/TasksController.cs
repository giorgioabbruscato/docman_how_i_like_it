using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Tasks.Application.Commands;
using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Application.Queries;
using HrPortal.Tasks.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Project task CRUD operations.</summary>
[ApiController]
[Route("api/v1/tasks")]
[Tags("Tasks")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class TasksController : ControllerBase
{
    private readonly GetProjectTasksQueryHandler _getTasksHandler;
    private readonly GetProjectTaskByIdQueryHandler _getTaskByIdHandler;
    private readonly CreateProjectTaskCommandHandler _createTaskHandler;
    private readonly UpdateProjectTaskCommandHandler _updateTaskHandler;
    private readonly DeleteProjectTaskCommandHandler _deleteTaskHandler;
    private readonly UpdateTaskStatusCommandHandler _updateTaskStatusHandler;

    public TasksController(
        GetProjectTasksQueryHandler getTasksHandler,
        GetProjectTaskByIdQueryHandler getTaskByIdHandler,
        CreateProjectTaskCommandHandler createTaskHandler,
        UpdateProjectTaskCommandHandler updateTaskHandler,
        DeleteProjectTaskCommandHandler deleteTaskHandler,
        UpdateTaskStatusCommandHandler updateTaskStatusHandler)
    {
        _getTasksHandler = getTasksHandler;
        _getTaskByIdHandler = getTaskByIdHandler;
        _createTaskHandler = createTaskHandler;
        _updateTaskHandler = updateTaskHandler;
        _deleteTaskHandler = deleteTaskHandler;
        _updateTaskStatusHandler = updateTaskStatusHandler;
    }

    /// <summary>List tasks with pagination and filters.</summary>
    /// <remarks>Auth: task.read:tenant</remarks>
    [HttpGet]
    [RequirePermission(Permissions.TaskReadTenant)]
    [ProducesResponseType(typeof(PagedResult<ProjectTaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetProjectTasksQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _getTasksHandler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get task by ID.</summary>
    /// <remarks>Auth: task.read:tenant</remarks>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.TaskReadTenant)]
    [ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getTaskByIdHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a new task.</summary>
    /// <remarks>Auth: task.create:tenant</remarks>
    [HttpPost]
    [RequirePermission(Permissions.TaskCreateTenant)]
    [ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createTaskHandler.HandleAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Update a task.</summary>
    /// <remarks>Auth: task.update:tenant</remarks>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.TaskUpdateTenant)]
    [ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectTaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateTaskHandler.HandleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Delete a task.</summary>
    /// <remarks>Auth: task.delete:tenant</remarks>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.TaskDeleteTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteTaskHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    /// <summary>Update task status only (for Kanban drag-and-drop).</summary>
    /// <remarks>Auth: task.update:tenant OR task.update_status:self</remarks>
    [HttpPatch("{id:guid}/status")]
    [RequireAnyPermission(Permissions.TaskUpdateTenant, Permissions.TaskUpdateStatusSelf)]
    [ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateTaskStatusHandler.HandleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = result.Error
            }),
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
