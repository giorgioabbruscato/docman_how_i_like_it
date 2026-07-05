using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.Tasks.Domain;

public sealed class ProjectTask : AuditableEntity
{
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? AssignedEmployeeId { get; private set; }
    public TaskPriority Priority { get; private set; }
    public TaskStatus Status { get; private set; }
    public decimal? EstimatedHours { get; private set; }
    public decimal SpentHours { get; private set; }
    public DateOnly? DueDate { get; private set; }

    private ProjectTask() { }

    public static ProjectTask Create(
        Guid tenantId,
        Guid projectId,
        string title,
        TaskPriority priority,
        TaskStatus status = TaskStatus.Todo,
        string? description = null,
        Guid? assignedEmployeeId = null,
        decimal? estimatedHours = null,
        DateOnly? dueDate = null,
        Guid? createdBy = null)
    {
        ValidateTitle(title);
        ValidateHours(estimatedHours, 0);

        return new ProjectTask
        {
            ProjectId = projectId,
            Title = title,
            Description = description,
            AssignedEmployeeId = assignedEmployeeId,
            Priority = priority,
            Status = status,
            EstimatedHours = estimatedHours,
            SpentHours = 0,
            DueDate = dueDate,
            CreatedBy = createdBy
        }.Also(t => t.SetTenant(tenantId));
    }

    public void Update(
        Guid projectId,
        string title,
        TaskPriority priority,
        TaskStatus status,
        string? description,
        Guid? assignedEmployeeId,
        decimal? estimatedHours,
        decimal spentHours,
        DateOnly? dueDate,
        Guid? updatedBy)
    {
        ValidateTitle(title);
        ValidateHours(estimatedHours, spentHours);

        ProjectId = projectId;
        Title = title;
        Description = description;
        AssignedEmployeeId = assignedEmployeeId;
        Priority = priority;
        Status = status;
        EstimatedHours = estimatedHours;
        SpentHours = spentHours;
        DueDate = dueDate;
        MarkUpdated(updatedBy);
    }

    public void UpdateStatus(TaskStatus newStatus, Guid? updatedBy)
    {
        if (newStatus == Status)
            throw new DomainException("Task is already in the requested status.", "INVALID_TRANSITION");

        Status = newStatus;
        MarkUpdated(updatedBy);
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Task title is required.");

        if (title.Length > 300)
            throw new DomainException("Task title must not exceed 300 characters.");
    }

    private static void ValidateHours(decimal? estimatedHours, decimal spentHours)
    {
        if (estimatedHours.HasValue && estimatedHours.Value < 0)
            throw new DomainException("Estimated hours must be greater than or equal to zero.");

        if (spentHours < 0)
            throw new DomainException("Spent hours must be greater than or equal to zero.");
    }
}

internal static class ProjectTaskExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
