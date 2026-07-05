using HrPortal.Projects.Application.Dtos;
using HrPortal.Projects.Domain;

namespace HrPortal.Projects.Application;

internal static class ProjectMapping
{
    internal static ProjectDto ToDto(Project project) =>
        new(
            project.Id,
            project.Name,
            project.Description,
            project.CustomerName,
            project.Status,
            project.StartDate,
            project.EndDate,
            project.BudgetHours,
            project.BudgetCost,
            project.IsArchived);

    internal static ProjectMemberDto ToDto(ProjectMember member) =>
        new(
            member.Id,
            member.ProjectId,
            member.EmployeeId,
            member.Role,
            member.HourlyRate);
}
