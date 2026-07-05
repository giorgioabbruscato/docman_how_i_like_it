using HrPortal.AccessControl.Domain;
using HrPortal.Authorization;
using HrPortal.Projects.Application.Commands;
using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Application.Queries;
using HrPortal.Projects.Domain;
using HrPortal.Tasks.Application.Queries;
using TaskBoardDto = HrPortal.Tasks.Application.Dtos.TaskBoardDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Project CRUD and membership operations.</summary>
[ApiController]
[Route("api/v1/projects")]
[Tags("Projects")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class ProjectsController : ControllerBase
{
    private readonly GetProjectsQueryHandler _getProjectsHandler;
    private readonly GetProjectByIdQueryHandler _getProjectByIdHandler;
    private readonly CreateProjectCommandHandler _createProjectHandler;
    private readonly UpdateProjectCommandHandler _updateProjectHandler;
    private readonly DeleteProjectCommandHandler _deleteProjectHandler;
    private readonly GetProjectMembersQueryHandler _getProjectMembersHandler;
    private readonly AddProjectMemberCommandHandler _addProjectMemberHandler;
    private readonly RemoveProjectMemberCommandHandler _removeProjectMemberHandler;
    private readonly GetTaskBoardQueryHandler _getTaskBoardHandler;

    public ProjectsController(
        GetProjectsQueryHandler getProjectsHandler,
        GetProjectByIdQueryHandler getProjectByIdHandler,
        CreateProjectCommandHandler createProjectHandler,
        UpdateProjectCommandHandler updateProjectHandler,
        DeleteProjectCommandHandler deleteProjectHandler,
        GetProjectMembersQueryHandler getProjectMembersHandler,
        AddProjectMemberCommandHandler addProjectMemberHandler,
        RemoveProjectMemberCommandHandler removeProjectMemberHandler,
        GetTaskBoardQueryHandler getTaskBoardHandler)
    {
        _getProjectsHandler = getProjectsHandler;
        _getProjectByIdHandler = getProjectByIdHandler;
        _createProjectHandler = createProjectHandler;
        _updateProjectHandler = updateProjectHandler;
        _deleteProjectHandler = deleteProjectHandler;
        _getProjectMembersHandler = getProjectMembersHandler;
        _addProjectMemberHandler = addProjectMemberHandler;
        _removeProjectMemberHandler = removeProjectMemberHandler;
        _getTaskBoardHandler = getTaskBoardHandler;
    }

    /// <summary>List projects with pagination and filters.</summary>
    /// <remarks>Auth: project.read:tenant</remarks>
    [HttpGet]
    [RequirePermission(Permissions.ProjectReadTenant)]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetProjectsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _getProjectsHandler.HandleAsync(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Get project by ID.</summary>
    /// <remarks>Auth: project.read:tenant</remarks>
    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.ProjectReadTenant)]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getProjectByIdHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Create a new project.</summary>
    /// <remarks>Auth: project.create:tenant</remarks>
    [HttpPost]
    [RequirePermission(Permissions.ProjectCreateTenant)]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createProjectHandler.HandleAsync(request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Update a project.</summary>
    /// <remarks>Auth: project.update:tenant</remarks>
    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.ProjectUpdateTenant)]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _updateProjectHandler.HandleAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Archive a project (soft delete).</summary>
    /// <remarks>Auth: project.delete:tenant</remarks>
    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.ProjectDeleteTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteProjectHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    /// <summary>List project members.</summary>
    /// <remarks>Auth: project.read:tenant</remarks>
    [HttpGet("{id:guid}/members")]
    [RequirePermission(Permissions.ProjectReadTenant)]
    [ProducesResponseType(typeof(IEnumerable<ProjectMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getProjectMembersHandler.HandleAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Add a member to a project.</summary>
    /// <remarks>Auth: project.manage_members:tenant</remarks>
    [HttpPost("{id:guid}/members")]
    [RequirePermission(Permissions.ProjectManageMembersTenant)]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddMember(
        Guid id,
        [FromBody] AddProjectMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _addProjectMemberHandler.HandleAsync(id, request, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetMembers), new { id }, result.Value)
            : MapFailure(result);
    }

    /// <summary>Remove a member from a project.</summary>
    /// <remarks>Auth: project.manage_members:tenant</remarks>
    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    [RequirePermission(Permissions.ProjectManageMembersTenant)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        Guid id,
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await _removeProjectMemberHandler.HandleAsync(id, memberId, cancellationToken);
        return result.IsSuccess ? NoContent() : MapFailure(result);
    }

    /// <summary>Get Kanban board for a project (tasks grouped by status).</summary>
    /// <remarks>Auth: task.read:tenant</remarks>
    [HttpGet("{id:guid}/tasks/board")]
    [RequirePermission(Permissions.TaskReadTenant)]
    [ProducesResponseType(typeof(TaskBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskBoard(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getTaskBoardHandler.HandleAsync(id, cancellationToken);
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
