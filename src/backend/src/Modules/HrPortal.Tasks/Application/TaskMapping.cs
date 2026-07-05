using HrPortal.Tasks.Application.Dtos;
using HrPortal.Tasks.Domain;

namespace HrPortal.Tasks.Application;

internal static class TaskMapping
{
    internal static ProjectTaskDto ToDto(ProjectTask task) =>
        new(
            task.Id,
            task.ProjectId,
            task.Title,
            task.Description,
            task.AssignedEmployeeId,
            task.Priority,
            task.Status,
            task.EstimatedHours,
            task.SpentHours,
            task.DueDate);
}
