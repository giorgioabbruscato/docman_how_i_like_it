using HrPortal.Projects.Domain;

namespace HrPortal.Projects.Application.Dtos;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string? CustomerName,
    ProjectStatus Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    decimal? BudgetHours,
    decimal? BudgetCost,
    bool IsArchived);

public sealed record ProjectMemberDto(
    Guid Id,
    Guid ProjectId,
    Guid EmployeeId,
    ProjectMemberRole Role,
    decimal? HourlyRate);

public sealed record CreateProjectRequest(
    string Name,
    ProjectStatus Status = ProjectStatus.Active,
    string? Description = null,
    string? CustomerName = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    decimal? BudgetHours = null,
    decimal? BudgetCost = null);

public sealed record UpdateProjectRequest(
    string Name,
    ProjectStatus Status,
    string? Description = null,
    string? CustomerName = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    decimal? BudgetHours = null,
    decimal? BudgetCost = null);

public sealed record AddProjectMemberRequest(
    Guid EmployeeId,
    ProjectMemberRole Role,
    decimal? HourlyRate = null);

public sealed record GetProjectsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? CustomerName = null,
    ProjectStatus? Status = null,
    bool? IsArchived = null);
