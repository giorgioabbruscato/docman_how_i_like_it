using HrPortal.Analytics.Application.Dtos;
using HrPortal.Analytics.Application.Options;
using HrPortal.Attendance.Application;
using HrPortal.Departments.Application;
using HrPortal.Employees.Application;
using HrPortal.Leave.Application;
using HrPortal.Projects.Application;
using HrPortal.SharedKernel.Results;
using HrPortal.Tasks.Application;
using HrPortal.Tenancy;
using HrPortal.TimeTracking.Application;
using Microsoft.Extensions.Options;

namespace HrPortal.Analytics.Application;

public interface IAnalyticsKpiService
{
    Task<Result<AnalyticsFilter>> BuildFilterAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> GetTotalWorkedHoursAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerEmployeeAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerDepartmentAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerProjectAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerCustomerAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MonthHoursRow>>> GetMonthlyTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DateHoursRow>>> GetDailyTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> GetAverageHoursPerDayAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> GetAttendanceRateAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> GetLeaveRateAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<decimal>> GetOvertimeHoursAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LateCheckInRow>>> GetLateCheckInsAsync(
        AnalyticsFilter filter,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<DateHoursRow>>> GetDailyAttendanceTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MonthHoursRow>>> GetMonthlyLeaveTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<EmployeeWorkingDto>>> GetEmployeesWorkingAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<AttendanceTodayDto>>> GetAttendanceTodayAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TopEmployeeDto>>> GetTopEmployeesAsync(
        AnalyticsFilter filter,
        int top = 5,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TopProjectDto>>> GetTopProjectsAsync(
        AnalyticsFilter filter,
        int top = 5,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<BudgetUsageDto>>> GetBudgetUsageAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<LateArrivalDto>>> GetLateArrivalsTodayAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<OvertimeEmployeeDto>>> GetOvertimeByEmployeeAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);

    Task<Result<SupervisorSummaryDto>> GetSupervisorSummaryAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default);
}

internal sealed class AnalyticsKpiService : IAnalyticsKpiService
{
    private readonly TenantContext _tenantContext;
    private readonly IEmployeeLookup _employeeLookup;
    private readonly IDepartmentLookup _departmentLookup;
    private readonly IProjectLookup _projectLookup;
    private readonly ITimeEntryAnalyticsProvider _timeEntryProvider;
    private readonly IAttendanceAnalyticsProvider _attendanceProvider;
    private readonly ILeaveAnalyticsProvider _leaveProvider;
    private readonly IProjectAnalyticsProvider _projectProvider;
    private readonly ITaskAnalyticsProvider _taskProvider;
    private readonly AnalyticsOptions _options;

    public AnalyticsKpiService(
        TenantContext tenantContext,
        IEmployeeLookup employeeLookup,
        IDepartmentLookup departmentLookup,
        IProjectLookup projectLookup,
        ITimeEntryAnalyticsProvider timeEntryProvider,
        IAttendanceAnalyticsProvider attendanceProvider,
        ILeaveAnalyticsProvider leaveProvider,
        IProjectAnalyticsProvider projectProvider,
        ITaskAnalyticsProvider taskProvider,
        IOptions<AnalyticsOptions> options)
    {
        _tenantContext = tenantContext;
        _employeeLookup = employeeLookup;
        _departmentLookup = departmentLookup;
        _projectLookup = projectLookup;
        _timeEntryProvider = timeEntryProvider;
        _attendanceProvider = attendanceProvider;
        _leaveProvider = leaveProvider;
        _projectProvider = projectProvider;
        _taskProvider = taskProvider;
        _options = options.Value;
    }

    public async Task<Result<AnalyticsFilter>> BuildFilterAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var scopeResult = await AnalyticsReadScope.ResolveAsync(
            _tenantContext,
            _employeeLookup,
            query.EmployeeId,
            cancellationToken);

        if (!scopeResult.IsSuccess)
            return Result.Failure<AnalyticsFilter>(scopeResult.Error!, scopeResult.ErrorCode);

        var (from, to) = ResolveDateRange(query.FromDate, query.ToDate);
        if (from > to)
            return Result.Failure<AnalyticsFilter>("FromDate must be on or before ToDate.", "VALIDATION_ERROR");

        var allowedEmployeeIds = await ResolveAllowedEmployeeIdsAsync(
            scopeResult.Value!,
            query.DepartmentId,
            cancellationToken);

        return Result.Success(new AnalyticsFilter(
            from,
            to,
            query.DepartmentId,
            query.ProjectId,
            query.EmployeeId ?? scopeResult.Value!.EmployeeId,
            allowedEmployeeIds));
    }

    public async Task<Result<decimal>> GetTotalWorkedHoursAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var minutes = await _timeEntryProvider.GetTotalMinutesAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        return Result.Success(MinutesToHours(minutes));
    }

    public async Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerEmployeeAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await _timeEntryProvider.GetMinutesByEmployeeAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var result = new List<NamedHoursRow>();
        foreach (var row in rows)
        {
            var name = await _employeeLookup.GetFullNameAsync(row.Id, cancellationToken) ?? "Unknown";
            result.Add(new NamedHoursRow(name, row.Id, MinutesToHours(row.Minutes)));
        }

        return Result.Success<IReadOnlyList<NamedHoursRow>>(result.OrderByDescending(r => r.Hours).ToList());
    }

    public async Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerDepartmentAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var employeeRows = await _timeEntryProvider.GetMinutesByEmployeeAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        if (employeeRows.Count == 0)
            return Result.Success<IReadOnlyList<NamedHoursRow>>([]);

        var departmentIds = await _employeeLookup.GetDepartmentIdsAsync(
            employeeRows.Select(r => r.Id).ToList(),
            cancellationToken);

        var byDepartment = new Dictionary<string, (Guid? DepartmentId, int Minutes)>();
        foreach (var row in employeeRows)
        {
            var departmentId = departmentIds.GetValueOrDefault(row.Id);
            var key = departmentId?.ToString() ?? "unassigned";
            if (byDepartment.TryGetValue(key, out var current))
                byDepartment[key] = (departmentId, current.Minutes + row.Minutes);
            else
                byDepartment[key] = (departmentId, row.Minutes);
        }

        var result = new List<NamedHoursRow>();
        foreach (var entry in byDepartment.Values)
        {
            var departmentId = entry.DepartmentId;
            var minutes = entry.Minutes;
            var label = departmentId.HasValue
                ? await _departmentLookup.GetNameAsync(departmentId.Value, cancellationToken) ?? "Unknown"
                : "Unassigned";
            result.Add(new NamedHoursRow(label, departmentId, MinutesToHours(minutes)));
        }

        return Result.Success<IReadOnlyList<NamedHoursRow>>(result.OrderByDescending(r => r.Hours).ToList());
    }

    public async Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerProjectAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await _timeEntryProvider.GetMinutesByProjectAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var result = new List<NamedHoursRow>();
        foreach (var row in rows)
        {
            var name = await _projectLookup.GetNameAsync(row.Id, cancellationToken) ?? "Unknown";
            result.Add(new NamedHoursRow(name, row.Id, MinutesToHours(row.Minutes)));
        }

        return Result.Success<IReadOnlyList<NamedHoursRow>>(result.OrderByDescending(r => r.Hours).ToList());
    }

    public async Task<Result<IReadOnlyList<NamedHoursRow>>> GetHoursPerCustomerAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var projectRows = await _timeEntryProvider.GetMinutesByProjectAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        if (projectRows.Count == 0)
            return Result.Success<IReadOnlyList<NamedHoursRow>>([]);

        var snapshots = await _projectProvider.GetBudgetSnapshotsAsync(filter.ProjectId, cancellationToken);
        var customerByProject = snapshots.ToDictionary(s => s.ProjectId, s => s.CustomerName ?? "Unknown");

        var byCustomer = new Dictionary<string, int>();
        foreach (var row in projectRows)
        {
            var customer = customerByProject.GetValueOrDefault(row.Id) ?? "Unknown";
            byCustomer.TryGetValue(customer, out var current);
            byCustomer[customer] = current + row.Minutes;
        }

        var result = byCustomer
            .Select(kvp => new NamedHoursRow(kvp.Key, null, MinutesToHours(kvp.Value)))
            .OrderByDescending(r => r.Hours)
            .ToList();

        return Result.Success<IReadOnlyList<NamedHoursRow>>(result);
    }

    public async Task<Result<IReadOnlyList<MonthHoursRow>>> GetMonthlyTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await _timeEntryProvider.GetMinutesByMonthAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        return Result.Success<IReadOnlyList<MonthHoursRow>>(
            rows.Select(r => new MonthHoursRow(r.Year, r.Month, MinutesToHours(r.Minutes))).ToList());
    }

    public async Task<Result<IReadOnlyList<DateHoursRow>>> GetDailyTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await _timeEntryProvider.GetMinutesByDayAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        return Result.Success<IReadOnlyList<DateHoursRow>>(
            rows.Select(r => new DateHoursRow(r.Date, MinutesToHours(r.Minutes))).ToList());
    }

    public async Task<Result<decimal>> GetAverageHoursPerDayAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var daily = await GetDailyTrendAsync(filter, cancellationToken);
        if (!daily.IsSuccess)
            return Result.Failure<decimal>(daily.Error!, daily.ErrorCode);

        var workdays = CountWorkdays(filter.From, filter.To);
        if (workdays == 0 || daily.Value!.Count == 0)
            return Result.Success(0m);

        var totalHours = daily.Value.Sum(d => d.Hours);
        return Result.Success(Math.Round(totalHours / workdays, 2));
    }

    public async Task<Result<decimal>> GetAttendanceRateAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var presentDays = await _attendanceProvider.GetPresentEmployeeDaysAsync(
            filter.From,
            filter.To,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var workdays = CountWorkdays(filter.From, filter.To);
        var employeeCount = await _employeeLookup.CountActiveEmployeesAsync(filter.AllowedEmployeeIds, cancellationToken);
        var denominator = workdays * employeeCount;

        if (denominator == 0)
            return Result.Success(0m);

        return Result.Success(Math.Round((decimal)presentDays / denominator, 4));
    }

    public async Task<Result<decimal>> GetLeaveRateAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var leaveDays = await _leaveProvider.GetApprovedLeaveDaysAsync(
            filter.From,
            filter.To,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var workdays = CountWorkdays(filter.From, filter.To);
        var employeeCount = await _employeeLookup.CountActiveEmployeesAsync(filter.AllowedEmployeeIds, cancellationToken);
        var denominator = workdays * employeeCount;

        if (denominator == 0)
            return Result.Success(0m);

        return Result.Success(Math.Round((decimal)leaveDays / denominator, 4));
    }

    public async Task<Result<decimal>> GetOvertimeHoursAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var minutes = await _timeEntryProvider.GetOvertimeMinutesAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            _options.DailyStandardMinutes,
            cancellationToken);

        return Result.Success(MinutesToHours(minutes));
    }

    public async Task<Result<IReadOnlyList<LateCheckInRow>>> GetLateCheckInsAsync(
        AnalyticsFilter filter,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var sessions = await _attendanceProvider.GetLateCheckInsAsync(
            date,
            _options.LateCheckInTime,
            filter.AllowedEmployeeIds,
            cancellationToken);

        if (filter.EmployeeId.HasValue)
            sessions = sessions.Where(s => s.EmployeeId == filter.EmployeeId.Value).ToList();

        var result = new List<LateCheckInRow>();
        foreach (var session in sessions.OrderBy(s => s.CheckIn))
        {
            var name = await _employeeLookup.GetFullNameAsync(session.EmployeeId, cancellationToken) ?? "Unknown";
            result.Add(new LateCheckInRow(session.EmployeeId, name, session.CheckIn));
        }

        return Result.Success<IReadOnlyList<LateCheckInRow>>(result);
    }

    public async Task<Result<IReadOnlyList<DateHoursRow>>> GetDailyAttendanceTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var presentCounts = await _attendanceProvider.GetDailyPresentCountsAsync(
            filter.From,
            filter.To,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var employeeCount = await _employeeLookup.CountActiveEmployeesAsync(filter.AllowedEmployeeIds, cancellationToken);
        if (employeeCount == 0)
            return Result.Success<IReadOnlyList<DateHoursRow>>([]);

        var workdaySet = _options.Workdays.ToHashSet();
        var presentLookup = presentCounts.ToDictionary(p => p.Date, p => p.Count);
        var result = new List<DateHoursRow>();

        for (var date = filter.From; date <= filter.To; date = date.AddDays(1))
        {
            if (!workdaySet.Contains(date.DayOfWeek))
                continue;

            var present = presentLookup.GetValueOrDefault(date);
            var rate = Math.Round((decimal)present / employeeCount, 4);
            result.Add(new DateHoursRow(date, rate));
        }

        return Result.Success<IReadOnlyList<DateHoursRow>>(result);
    }

    public async Task<Result<IReadOnlyList<MonthHoursRow>>> GetMonthlyLeaveTrendAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var rows = await _leaveProvider.GetMonthlyLeaveTrendAsync(
            filter.From,
            filter.To,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        return Result.Success<IReadOnlyList<MonthHoursRow>>(
            rows.Select(r => new MonthHoursRow(r.Year, r.Month, r.Days)).ToList());
    }

    public async Task<Result<IReadOnlyList<EmployeeWorkingDto>>> GetEmployeesWorkingAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var openSessions = await _attendanceProvider.GetOpenSessionsAsync(filter.AllowedEmployeeIds, cancellationToken);
        var activeTimers = await _timeEntryProvider.GetActiveTimersAsync(
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var working = new Dictionary<Guid, EmployeeWorkingDto>();

        foreach (var session in openSessions)
        {
            if (filter.EmployeeId.HasValue && session.EmployeeId != filter.EmployeeId.Value)
                continue;

            var name = await _employeeLookup.GetFullNameAsync(session.EmployeeId, cancellationToken) ?? "Unknown";
            working[session.EmployeeId] = new EmployeeWorkingDto(
                session.EmployeeId,
                name,
                null,
                null,
                session.CheckIn);
        }

        foreach (var timer in activeTimers)
        {
            if (filter.EmployeeId.HasValue && timer.EmployeeId != filter.EmployeeId.Value)
                continue;

            var name = await _employeeLookup.GetFullNameAsync(timer.EmployeeId, cancellationToken) ?? "Unknown";
            var projectName = await _projectLookup.GetNameAsync(timer.ProjectId, cancellationToken);
            working[timer.EmployeeId] = new EmployeeWorkingDto(
                timer.EmployeeId,
                name,
                timer.ProjectId,
                projectName,
                working.GetValueOrDefault(timer.EmployeeId)?.CheckInTime);
        }

        return Result.Success<IReadOnlyList<EmployeeWorkingDto>>(
            working.Values.OrderBy(e => e.EmployeeName).ToList());
    }

    public async Task<Result<IReadOnlyList<AttendanceTodayDto>>> GetAttendanceTodayAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var sessions = await _attendanceProvider.GetSessionsInRangeAsync(
            today,
            today,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var earliestByEmployee = sessions
            .GroupBy(s => s.EmployeeId)
            .Select(g => g.OrderBy(s => s.CheckIn).First())
            .OrderBy(s => s.CheckIn)
            .ToList();

        var result = new List<AttendanceTodayDto>();
        foreach (var session in earliestByEmployee)
        {
            var name = await _employeeLookup.GetFullNameAsync(session.EmployeeId, cancellationToken) ?? "Unknown";
            result.Add(new AttendanceTodayDto(session.EmployeeId, name, session.CheckIn));
        }

        return Result.Success<IReadOnlyList<AttendanceTodayDto>>(result);
    }

    public async Task<Result<IReadOnlyList<TopEmployeeDto>>> GetTopEmployeesAsync(
        AnalyticsFilter filter,
        int top = 5,
        CancellationToken cancellationToken = default)
    {
        var hoursResult = await GetHoursPerEmployeeAsync(filter, cancellationToken);
        if (!hoursResult.IsSuccess)
            return Result.Failure<IReadOnlyList<TopEmployeeDto>>(hoursResult.Error!, hoursResult.ErrorCode);

        var topRows = hoursResult.Value!
            .Take(top)
            .Select(r => new TopEmployeeDto(r.Id!.Value, r.Label, r.Hours))
            .ToList();

        return Result.Success<IReadOnlyList<TopEmployeeDto>>(topRows);
    }

    public async Task<Result<IReadOnlyList<TopProjectDto>>> GetTopProjectsAsync(
        AnalyticsFilter filter,
        int top = 5,
        CancellationToken cancellationToken = default)
    {
        var hoursResult = await GetHoursPerProjectAsync(filter, cancellationToken);
        if (!hoursResult.IsSuccess)
            return Result.Failure<IReadOnlyList<TopProjectDto>>(hoursResult.Error!, hoursResult.ErrorCode);

        var topRows = hoursResult.Value!
            .Take(top)
            .Select(r => new TopProjectDto(r.Id!.Value, r.Label, r.Hours))
            .ToList();

        return Result.Success<IReadOnlyList<TopProjectDto>>(topRows);
    }

    public async Task<Result<IReadOnlyList<BudgetUsageDto>>> GetBudgetUsageAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var snapshots = await _projectProvider.GetBudgetSnapshotsAsync(filter.ProjectId, cancellationToken);
        var timeByProject = await _timeEntryProvider.GetMinutesByProjectAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);
        var taskSpent = await _taskProvider.GetSpentHoursByProjectAsync(filter.ProjectId, cancellationToken);
        var memberRates = await _projectProvider.GetMemberHourlyRatesAsync(
            snapshots.Select(s => s.ProjectId).ToList(),
            cancellationToken);

        var timeLookup = timeByProject.ToDictionary(r => r.Id, r => MinutesToHours(r.Minutes));
        var taskLookup = taskSpent.ToDictionary(r => r.ProjectId, r => r.SpentHours);

        var result = new List<BudgetUsageDto>();
        foreach (var snapshot in snapshots.OrderBy(s => s.Name))
        {
            var timeHours = timeLookup.GetValueOrDefault(snapshot.ProjectId);
            var taskHours = taskLookup.GetValueOrDefault(snapshot.ProjectId);
            var spentHours = timeHours + taskHours;

            decimal? actualCost = null;
            if (snapshot.BudgetCost.HasValue || memberRates.Any(r => r.ProjectId == snapshot.ProjectId))
            {
                actualCost = await CalculateActualCostAsync(
                    snapshot.ProjectId,
                    filter,
                    memberRates,
                    cancellationToken);
            }

            result.Add(new BudgetUsageDto(
                snapshot.ProjectId,
                snapshot.Name,
                snapshot.BudgetHours,
                spentHours,
                snapshot.BudgetCost,
                actualCost));
        }

        return Result.Success<IReadOnlyList<BudgetUsageDto>>(result);
    }

    public async Task<Result<IReadOnlyList<LateArrivalDto>>> GetLateArrivalsTodayAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var lateResult = await GetLateCheckInsAsync(filter, today, cancellationToken);
        if (!lateResult.IsSuccess)
            return Result.Failure<IReadOnlyList<LateArrivalDto>>(lateResult.Error!, lateResult.ErrorCode);

        var result = lateResult.Value!
            .Select(r => new LateArrivalDto(r.EmployeeId, r.EmployeeName, r.CheckInTime))
            .ToList();

        return Result.Success<IReadOnlyList<LateArrivalDto>>(result);
    }

    public async Task<Result<IReadOnlyList<OvertimeEmployeeDto>>> GetOvertimeByEmployeeAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var entries = await _timeEntryProvider.GetMinutesByEmployeeAsync(
            filter.From,
            filter.To,
            filter.ProjectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        var employeeIds = entries.Select(e => e.Id).Distinct().ToList();
        var overtimeRows = new List<OvertimeEmployeeDto>();

        foreach (var employeeId in employeeIds)
        {
            var employeeFilter = filter with { EmployeeId = employeeId };
            var daily = await _timeEntryProvider.GetMinutesByDayAsync(
                employeeFilter.From,
                employeeFilter.To,
                employeeFilter.ProjectId,
                employeeId,
                employeeFilter.AllowedEmployeeIds,
                cancellationToken);

            var overtimeMinutes = daily
                .Where(d => d.Minutes > _options.DailyStandardMinutes)
                .Sum(d => d.Minutes - _options.DailyStandardMinutes);

            if (overtimeMinutes <= 0)
                continue;

            var name = await _employeeLookup.GetFullNameAsync(employeeId, cancellationToken) ?? "Unknown";
            overtimeRows.Add(new OvertimeEmployeeDto(employeeId, name, MinutesToHours(overtimeMinutes)));
        }

        return Result.Success<IReadOnlyList<OvertimeEmployeeDto>>(
            overtimeRows.OrderByDescending(r => r.OvertimeHours).ToList());
    }

    public async Task<Result<SupervisorSummaryDto>> GetSupervisorSummaryAsync(
        AnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        var employeesWorking = await GetEmployeesWorkingAsync(filter, cancellationToken);
        var attendanceToday = await GetAttendanceTodayAsync(filter, cancellationToken);
        var topEmployees = await GetTopEmployeesAsync(filter, cancellationToken: cancellationToken);
        var topProjects = await GetTopProjectsAsync(filter, cancellationToken: cancellationToken);
        var budgetUsage = await GetBudgetUsageAsync(filter, cancellationToken);
        var lateArrivals = await GetLateArrivalsTodayAsync(filter, cancellationToken);
        var overtime = await GetOvertimeByEmployeeAsync(filter, cancellationToken);
        var totalHours = await GetTotalWorkedHoursAsync(filter, cancellationToken);
        var attendanceRate = await GetAttendanceRateAsync(filter, cancellationToken);
        var leaveRate = await GetLeaveRateAsync(filter, cancellationToken);

        if (!employeesWorking.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(employeesWorking.Error!, employeesWorking.ErrorCode);
        if (!attendanceToday.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(attendanceToday.Error!, attendanceToday.ErrorCode);
        if (!topEmployees.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(topEmployees.Error!, topEmployees.ErrorCode);
        if (!topProjects.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(topProjects.Error!, topProjects.ErrorCode);
        if (!budgetUsage.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(budgetUsage.Error!, budgetUsage.ErrorCode);
        if (!lateArrivals.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(lateArrivals.Error!, lateArrivals.ErrorCode);
        if (!overtime.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(overtime.Error!, overtime.ErrorCode);
        if (!totalHours.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(totalHours.Error!, totalHours.ErrorCode);
        if (!attendanceRate.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(attendanceRate.Error!, attendanceRate.ErrorCode);
        if (!leaveRate.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(leaveRate.Error!, leaveRate.ErrorCode);

        return Result.Success(new SupervisorSummaryDto(
            employeesWorking.Value!,
            attendanceToday.Value!,
            topEmployees.Value!,
            topProjects.Value!,
            budgetUsage.Value!,
            lateArrivals.Value!,
            overtime.Value!,
            totalHours.Value,
            attendanceRate.Value,
            leaveRate.Value));
    }

    private async Task<decimal?> CalculateActualCostAsync(
        Guid projectId,
        AnalyticsFilter filter,
        IReadOnlyList<ProjectMemberRateRow> memberRates,
        CancellationToken cancellationToken)
    {
        var rates = memberRates
            .Where(r => r.ProjectId == projectId && r.HourlyRate.HasValue)
            .ToDictionary(r => r.EmployeeId, r => r.HourlyRate!.Value);

        if (rates.Count == 0)
            return null;

        var byEmployee = await _timeEntryProvider.GetMinutesByEmployeeAsync(
            filter.From,
            filter.To,
            projectId,
            filter.EmployeeId,
            filter.AllowedEmployeeIds,
            cancellationToken);

        decimal total = 0m;
        foreach (var row in byEmployee)
        {
            if (rates.TryGetValue(row.Id, out var rate))
                total += MinutesToHours(row.Minutes) * rate;
        }

        return Math.Round(total, 2);
    }

    private async Task<IReadOnlyList<Guid>?> ResolveAllowedEmployeeIdsAsync(
        AnalyticsReadFilter scope,
        Guid? departmentId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid>? effective = scope.AllowedEmployeeIds;

        if (departmentId.HasValue)
        {
            var departmentEmployeeIds = await _employeeLookup.GetActiveEmployeeIdsInDepartmentAsync(
                departmentId.Value,
                cancellationToken);

            effective = effective is null
                ? departmentEmployeeIds
                : effective.Intersect(departmentEmployeeIds).ToList();
        }

        if (scope.EmployeeId.HasValue)
            effective = [scope.EmployeeId.Value];
        else if (effective is not null)
            effective = effective.ToList();

        return effective;
    }

    private int CountWorkdays(DateOnly from, DateOnly to)
    {
        var workdaySet = _options.Workdays.ToHashSet();
        var count = 0;

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            if (workdaySet.Contains(date.DayOfWeek))
                count++;
        }

        return count;
    }

    private static (DateOnly From, DateOnly To) ResolveDateRange(DateOnly? from, DateOnly? to)
    {
        var utcNow = DateTime.UtcNow;
        var defaultFrom = new DateOnly(utcNow.Year, utcNow.Month, 1);
        var defaultTo = new DateOnly(utcNow.Year, utcNow.Month, DateTime.DaysInMonth(utcNow.Year, utcNow.Month));

        return (from ?? defaultFrom, to ?? defaultTo);
    }

    private static decimal MinutesToHours(int minutes) =>
        Math.Round(minutes / 60m, 2);
}
