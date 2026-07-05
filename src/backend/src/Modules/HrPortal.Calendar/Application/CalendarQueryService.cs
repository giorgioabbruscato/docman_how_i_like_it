using HrPortal.Calendar.Application.Dtos;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Calendar.Application;

public interface ICalendarQueryService
{
    Task<Result<IReadOnlyList<CalendarEventDto>>> GetEventsAsync(
        GetCalendarEventsQuery query,
        CancellationToken cancellationToken = default);
}

internal sealed class CalendarQueryService : ICalendarQueryService
{
    private const int MaxRangeDays = 366;
    private const string ReadTeam = "calendar.read:team";

    private readonly ILeaveCalendarProvider _leaveProvider;
    private readonly ISmartWorkingCalendarProvider _smartWorkingProvider;
    private readonly IPublicHolidayRepository _holidayRepository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly TenantContext _tenantContext;

    public CalendarQueryService(
        ILeaveCalendarProvider leaveProvider,
        ISmartWorkingCalendarProvider smartWorkingProvider,
        IPublicHolidayRepository holidayRepository,
        IEmployeeLookup employeeLookup,
        TenantContext tenantContext)
    {
        _leaveProvider = leaveProvider;
        _smartWorkingProvider = smartWorkingProvider;
        _holidayRepository = holidayRepository;
        _employeeLookup = employeeLookup;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<CalendarEventDto>>> GetEventsAsync(
        GetCalendarEventsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.ToDate < query.FromDate)
            return Result.Failure<IReadOnlyList<CalendarEventDto>>("To date must be on or after from date.");

        if (query.ToDate.DayNumber - query.FromDate.DayNumber > MaxRangeDays)
            return Result.Failure<IReadOnlyList<CalendarEventDto>>($"Date range cannot exceed {MaxRangeDays} days.");

        var employeeIds = await ResolveEmployeeIdsAsync(query, cancellationToken);
        if (!employeeIds.IsSuccess)
            return Result.Failure<IReadOnlyList<CalendarEventDto>>(employeeIds.Error!, employeeIds.ErrorCode);

        var events = new List<CalendarEventDto>();

        events.AddRange(await _leaveProvider.GetApprovedInDateRangeAsync(
            query.FromDate, query.ToDate, employeeIds.Value, cancellationToken));

        events.AddRange(await _smartWorkingProvider.GetEventsInDateRangeAsync(
            query.FromDate, query.ToDate, employeeIds.Value, cancellationToken));

        var holidays = await _holidayRepository.GetInDateRangeAsync(
            query.FromDate, query.ToDate, cancellationToken);

        foreach (var holiday in holidays)
        {
            events.Add(new CalendarEventDto(
                $"holiday:{holiday.Id}",
                holiday.Name,
                holiday.Date,
                holiday.Date,
                CalendarEventType.Holiday,
                null,
                null,
                "#f59e0b"));
        }

        return Result.Success<IReadOnlyList<CalendarEventDto>>(events.OrderBy(e => e.StartDate).ToList());
    }

    private async Task<Result<IReadOnlyList<Guid>?>> ResolveEmployeeIdsAsync(
        GetCalendarEventsQuery query,
        CancellationToken cancellationToken)
    {
        if (_tenantContext.HasPermission(ReadTeam))
        {
            if (query.EmployeeId.HasValue)
                return Result.Success<IReadOnlyList<Guid>?>([query.EmployeeId.Value]);

            if (query.DepartmentId.HasValue)
            {
                var ids = await _employeeLookup.GetActiveEmployeeIdsInDepartmentAsync(
                    query.DepartmentId.Value, cancellationToken);
                return Result.Success<IReadOnlyList<Guid>?>(ids);
            }

            return Result.Success<IReadOnlyList<Guid>?>(null);
        }

        if (!_tenantContext.EmployeeId.HasValue)
            return Result.Failure<IReadOnlyList<Guid>?>("Employee context is required.", "FORBIDDEN");

        return Result.Success<IReadOnlyList<Guid>?>([_tenantContext.EmployeeId.Value]);
    }
}
