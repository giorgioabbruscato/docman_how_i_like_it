using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;

namespace HrPortal.TimeTracking.Application;

public static class TimeEntryMapping
{
    public static TimeEntryDto ToDto(TimeEntry entry) =>
        new(
            entry.Id,
            entry.EmployeeId,
            entry.ProjectId,
            entry.TaskId,
            entry.StartTime,
            entry.EndTime,
            entry.WorkedMinutes,
            entry.Description,
            entry.Billable);
}
