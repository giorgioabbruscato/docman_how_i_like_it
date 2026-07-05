using HrPortal.Tasks.Domain;
using DomainTaskStatus = HrPortal.Tasks.Domain.TaskStatus;

namespace HrPortal.Tasks.Application.Dtos;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record ProjectTaskDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    Guid? AssignedEmployeeId,
    TaskPriority Priority,
    DomainTaskStatus Status,
    decimal? EstimatedHours,
    decimal SpentHours,
    DateOnly? DueDate);

public sealed record CreateProjectTaskRequest(
    Guid ProjectId,
    string Title,
    TaskPriority Priority = TaskPriority.Medium,
    DomainTaskStatus Status = DomainTaskStatus.Todo,
    string? Description = null,
    Guid? AssignedEmployeeId = null,
    decimal? EstimatedHours = null,
    DateOnly? DueDate = null);

public sealed record UpdateProjectTaskRequest(
    Guid ProjectId,
    string Title,
    TaskPriority Priority,
    DomainTaskStatus Status,
    string? Description = null,
    Guid? AssignedEmployeeId = null,
    decimal? EstimatedHours = null,
    decimal SpentHours = 0,
    DateOnly? DueDate = null);

public sealed record GetProjectTasksQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? ProjectId = null,
    DomainTaskStatus? Status = null,
    TaskPriority? Priority = null,
    Guid? AssignedEmployeeId = null);

public sealed record TaskBoardDto(Guid ProjectId, IReadOnlyList<TaskBoardColumnDto> Columns);

public sealed record TaskBoardColumnDto(DomainTaskStatus Status, IReadOnlyList<ProjectTaskDto> Tasks);

public sealed record UpdateTaskStatusRequest(DomainTaskStatus Status, DateTime? UpdatedAt = null);
