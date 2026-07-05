using HrPortal.SharedKernel.Entities;
using HrPortal.SharedKernel.Exceptions;

namespace HrPortal.TimeTracking.Domain;

public sealed class TimeEntry : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid? TaskId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public int WorkedMinutes { get; private set; }
    public string? Description { get; private set; }
    public bool Billable { get; private set; }

    private TimeEntry() { }

    public static TimeEntry Create(
        Guid tenantId,
        Guid employeeId,
        Guid projectId,
        DateTime startTime,
        DateTime endTime,
        Guid? taskId = null,
        string? description = null,
        bool billable = true,
        Guid? createdBy = null)
    {
        ValidateTimes(startTime, endTime);

        return new TimeEntry
        {
            EmployeeId = employeeId,
            ProjectId = projectId,
            TaskId = taskId,
            StartTime = startTime,
            EndTime = endTime,
            WorkedMinutes = CalculateWorkedMinutes(startTime, endTime),
            Description = description,
            Billable = billable,
            CreatedBy = createdBy
        }.Also(t => t.SetTenant(tenantId));
    }

    public static TimeEntry StartTimer(
        Guid tenantId,
        Guid employeeId,
        Guid projectId,
        DateTime startTime,
        Guid? taskId = null,
        string? description = null,
        bool billable = true,
        Guid? createdBy = null)
    {
        return new TimeEntry
        {
            EmployeeId = employeeId,
            ProjectId = projectId,
            TaskId = taskId,
            StartTime = startTime,
            EndTime = null,
            WorkedMinutes = 0,
            Description = description,
            Billable = billable,
            CreatedBy = createdBy
        }.Also(t => t.SetTenant(tenantId));
    }

    public void Stop(DateTime utcNow)
    {
        if (EndTime.HasValue)
            throw new DomainException("Timer is already stopped.", "CONFLICT");

        if (utcNow <= StartTime)
            throw new DomainException("Stop time must be after start time.");

        EndTime = utcNow;
        WorkedMinutes = CalculateWorkedMinutes(StartTime, utcNow);
        MarkUpdated(null);
    }

    public void Update(
        Guid projectId,
        DateTime startTime,
        DateTime endTime,
        Guid? taskId,
        string? description,
        bool billable,
        Guid? updatedBy)
    {
        ValidateTimes(startTime, endTime);

        ProjectId = projectId;
        TaskId = taskId;
        StartTime = startTime;
        EndTime = endTime;
        WorkedMinutes = CalculateWorkedMinutes(startTime, endTime);
        Description = description;
        Billable = billable;
        MarkUpdated(updatedBy);
    }

    public static int CalculateWorkedMinutes(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new DomainException("End time must be after start time.");

        return (int)Math.Round((end - start).TotalMinutes);
    }

    private static void ValidateTimes(DateTime startTime, DateTime endTime)
    {
        if (endTime <= startTime)
            throw new DomainException("End time must be after start time.");
    }
}

internal static class TimeEntryExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
