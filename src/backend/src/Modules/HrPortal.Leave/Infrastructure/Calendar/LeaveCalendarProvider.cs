using HrPortal.Calendar.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application;
using HrPortal.Leave.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Leave.Infrastructure.Calendar;

internal sealed class LeaveCalendarProvider : ILeaveCalendarProvider
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;
    private readonly IEmployeeLookup _employeeLookup;

    public LeaveCalendarProvider(
        DbContext dbContext,
        ITenantContextAccessor accessor,
        IEmployeeLookup employeeLookup)
    {
        _dbContext = dbContext;
        _accessor = accessor;
        _employeeLookup = employeeLookup;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> GetApprovedInDateRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<Guid>? employeeIds,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<LeaveRequest>()
            .ApplyTenantScope(_accessor.Current)
            .Where(l => l.Status == LeaveStatus.Approved
                        && l.EndDate >= fromDate
                        && l.StartDate <= toDate);

        if (employeeIds is not null)
            query = query.Where(l => employeeIds.Contains(l.EmployeeId));

        var requests = await query.ToListAsync(cancellationToken);
        var events = new List<CalendarEventDto>();

        foreach (var request in requests)
        {
            var name = await _employeeLookup.GetFullNameAsync(request.EmployeeId, cancellationToken);
            var type = request.Type == LeaveType.Personal
                ? CalendarEventType.Permission
                : CalendarEventType.Leave;
            var color = type == CalendarEventType.Permission ? "#06b6d4" : "#22c55e";

            events.Add(new CalendarEventDto(
                $"leave:{request.Id}",
                $"{request.Type} leave",
                request.StartDate,
                request.EndDate,
                type,
                request.EmployeeId,
                name,
                color));
        }

        return events;
    }
}
