using HrPortal.Attendance.Application.Dtos;
using HrPortal.Attendance.Domain;
using HrPortal.Employees.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Attendance.Application.Queries;

public sealed class GetAttendanceDashboardQueryHandler
{
    private readonly IAttendanceSessionRepository _repository;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly TenantContext _tenantContext;

    public GetAttendanceDashboardQueryHandler(
        IAttendanceSessionRepository repository,
        IEmployeeLookup employeeLookup,
        TenantContext tenantContext)
    {
        _repository = repository;
        _employeeLookup = employeeLookup;
        _tenantContext = tenantContext;
    }

    public async Task<Result<AttendanceDashboardDto>> HandleAsync(
        Guid? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await AttendanceSessionReadScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            employeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            return Result.Failure<AttendanceDashboardDto>(scopeResult.Error!, scopeResult.ErrorCode);

        var filter = scopeResult.Value!;
        var targetEmployeeId = filter.EmployeeId ?? _tenantContext.EmployeeId;
        if (!targetEmployeeId.HasValue)
            return Result.Failure<AttendanceDashboardDto>("Employee context is required.", "FORBIDDEN");

        var utcNow = DateTime.UtcNow;
        var todayStart = utcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        var todaySessions = filter.AllowedEmployeeIds is not null
            ? await _repository.GetByEmployeeIdsAndDateRangeAsync(
                filter.AllowedEmployeeIds,
                todayStart,
                todayEnd,
                cancellationToken)
            : await _repository.GetByEmployeeAndDateRangeAsync(
                targetEmployeeId.Value,
                todayStart,
                todayEnd,
                cancellationToken);

        var employeeTodaySessions = todaySessions
            .Where(s => s.EmployeeId == targetEmployeeId.Value)
            .ToList();

        var weekStart = GetWeekStartUtc(utcNow);
        var weekEnd = weekStart.AddDays(7);
        var monthStart = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var weekSessions = filter.AllowedEmployeeIds is not null
            ? await _repository.GetByEmployeeIdsAndDateRangeAsync(
                filter.AllowedEmployeeIds,
                weekStart,
                weekEnd,
                cancellationToken)
            : await _repository.GetByEmployeeAndDateRangeAsync(
                targetEmployeeId.Value,
                weekStart,
                weekEnd,
                cancellationToken);

        var monthSessions = filter.AllowedEmployeeIds is not null
            ? await _repository.GetByEmployeeIdsAndDateRangeAsync(
                filter.AllowedEmployeeIds,
                monthStart,
                monthEnd,
                cancellationToken)
            : await _repository.GetByEmployeeAndDateRangeAsync(
                targetEmployeeId.Value,
                monthStart,
                monthEnd,
                cancellationToken);

        var employeeWeekSessions = weekSessions.Where(s => s.EmployeeId == targetEmployeeId.Value).ToList();
        var employeeMonthSessions = monthSessions.Where(s => s.EmployeeId == targetEmployeeId.Value).ToList();

        var openSession = await _repository.GetOpenSessionAsync(targetEmployeeId.Value, cancellationToken);

        var todayCheckIn = employeeTodaySessions.Count > 0
            ? employeeTodaySessions.Min(s => s.CheckIn)
            : (DateTime?)null;

        var closedToday = employeeTodaySessions
            .Where(s => s.CheckOut.HasValue)
            .ToList();

        var todayCheckOut = closedToday.Count > 0
            ? closedToday.Max(s => s.CheckOut)
            : null;

        var todayWorkedMinutes = SumWorkedMinutes(employeeTodaySessions, utcNow);
        var weeklyTotalMinutes = SumWorkedMinutes(employeeWeekSessions, utcNow);
        var monthlyTotalMinutes = SumWorkedMinutes(employeeMonthSessions, utcNow);

        return Result.Success(new AttendanceDashboardDto(
            todayCheckIn,
            todayCheckOut,
            todayWorkedMinutes,
            openSession is null ? null : AttendanceSessionMapping.ToDto(openSession),
            weeklyTotalMinutes,
            monthlyTotalMinutes));
    }

    private static DateTime GetWeekStartUtc(DateTime utcNow)
    {
        var dayOfWeek = (int)utcNow.DayOfWeek;
        var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return utcNow.Date.AddDays(-daysFromMonday);
    }

    private static int SumWorkedMinutes(IReadOnlyList<AttendanceSession> sessions, DateTime utcNow)
    {
        var total = 0;

        foreach (var session in sessions)
        {
            if (session.Status == AttendanceSessionStatus.Open)
                total += AttendanceSession.CalculateWorkedMinutes(session.CheckIn, utcNow);
            else if (session.WorkedMinutes.HasValue)
                total += session.WorkedMinutes.Value;
        }

        return total;
    }
}
