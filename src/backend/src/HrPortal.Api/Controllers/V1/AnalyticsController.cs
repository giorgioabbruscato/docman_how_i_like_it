using HrPortal.AccessControl.Domain;
using HrPortal.Analytics.Application.Dtos;
using HrPortal.Analytics.Application.Queries;
using HrPortal.Authorization;
using HrPortal.Tenancy.Application;
using HrPortal.Tenancy.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Controllers.V1;

/// <summary>Analytics supervisor dashboard and chart endpoints.</summary>
[ApiController]
[Route("api/v1/analytics")]
[Tags("Analytics")]
[Authorize(Policy = Policies.Authenticated)]
[Produces("application/json")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IFeatureGateService _featureGateService;
    private readonly GetSupervisorSummaryQueryHandler _summaryHandler;
    private readonly GetEmployeesWorkingQueryHandler _employeesWorkingHandler;
    private readonly GetAttendanceTodayQueryHandler _attendanceTodayHandler;
    private readonly GetTopEmployeesQueryHandler _topEmployeesHandler;
    private readonly GetTopProjectsQueryHandler _topProjectsHandler;
    private readonly GetBudgetUsageQueryHandler _budgetUsageHandler;
    private readonly GetLateArrivalsQueryHandler _lateArrivalsHandler;
    private readonly GetOvertimeQueryHandler _overtimeHandler;
    private readonly GetHoursByProjectChartQueryHandler _hoursByProjectChartHandler;
    private readonly GetHoursByDepartmentChartQueryHandler _hoursByDepartmentChartHandler;
    private readonly GetHoursByEmployeeChartQueryHandler _hoursByEmployeeChartHandler;
    private readonly GetHoursByMonthChartQueryHandler _hoursByMonthChartHandler;
    private readonly GetAttendanceTrendChartQueryHandler _attendanceTrendChartHandler;
    private readonly GetLeaveTrendChartQueryHandler _leaveTrendChartHandler;
    private readonly GetBudgetConsumptionChartQueryHandler _budgetConsumptionChartHandler;

    public AnalyticsController(
        IFeatureGateService featureGateService,
        GetSupervisorSummaryQueryHandler summaryHandler,
        GetEmployeesWorkingQueryHandler employeesWorkingHandler,
        GetAttendanceTodayQueryHandler attendanceTodayHandler,
        GetTopEmployeesQueryHandler topEmployeesHandler,
        GetTopProjectsQueryHandler topProjectsHandler,
        GetBudgetUsageQueryHandler budgetUsageHandler,
        GetLateArrivalsQueryHandler lateArrivalsHandler,
        GetOvertimeQueryHandler overtimeHandler,
        GetHoursByProjectChartQueryHandler hoursByProjectChartHandler,
        GetHoursByDepartmentChartQueryHandler hoursByDepartmentChartHandler,
        GetHoursByEmployeeChartQueryHandler hoursByEmployeeChartHandler,
        GetHoursByMonthChartQueryHandler hoursByMonthChartHandler,
        GetAttendanceTrendChartQueryHandler attendanceTrendChartHandler,
        GetLeaveTrendChartQueryHandler leaveTrendChartHandler,
        GetBudgetConsumptionChartQueryHandler budgetConsumptionChartHandler)
    {
        _featureGateService = featureGateService;
        _summaryHandler = summaryHandler;
        _employeesWorkingHandler = employeesWorkingHandler;
        _attendanceTodayHandler = attendanceTodayHandler;
        _topEmployeesHandler = topEmployeesHandler;
        _topProjectsHandler = topProjectsHandler;
        _budgetUsageHandler = budgetUsageHandler;
        _lateArrivalsHandler = lateArrivalsHandler;
        _overtimeHandler = overtimeHandler;
        _hoursByProjectChartHandler = hoursByProjectChartHandler;
        _hoursByDepartmentChartHandler = hoursByDepartmentChartHandler;
        _hoursByEmployeeChartHandler = hoursByEmployeeChartHandler;
        _hoursByMonthChartHandler = hoursByMonthChartHandler;
        _attendanceTrendChartHandler = attendanceTrendChartHandler;
        _leaveTrendChartHandler = leaveTrendChartHandler;
        _budgetConsumptionChartHandler = budgetConsumptionChartHandler;
    }

    /// <summary>Supervisor dashboard summary with all widget sections.</summary>
    [HttpGet("supervisor/summary")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(SupervisorSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSupervisorSummary(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _summaryHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Employees currently working (open attendance or active timer).</summary>
    [HttpGet("supervisor/employees-working")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeWorkingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeesWorking(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _employeesWorkingHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Today's attendance check-ins for scoped employees.</summary>
    [HttpGet("supervisor/attendance-today")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(IReadOnlyList<AttendanceTodayDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceToday(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _attendanceTodayHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Top employees by worked hours in the selected range.</summary>
    [HttpGet("supervisor/top-employees")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(IReadOnlyList<TopEmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopEmployees(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int top = 5,
        CancellationToken cancellationToken = default)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _topEmployeesHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            top,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Top projects by worked hours in the selected range.</summary>
    [HttpGet("supervisor/top-projects")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(IReadOnlyList<TopProjectDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopProjects(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] int top = 5,
        CancellationToken cancellationToken = default)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _topProjectsHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            top,
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Project budget usage (tenant scope only).</summary>
    [HttpGet("supervisor/budget-usage")]
    [RequirePermission(Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetUsageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBudgetUsage(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _budgetUsageHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Late arrivals for today.</summary>
    [HttpGet("supervisor/late-arrivals")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(IReadOnlyList<LateArrivalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLateArrivals(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _lateArrivalsHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Overtime breakdown by employee in the selected range.</summary>
    [HttpGet("supervisor/overtime")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(IReadOnlyList<OvertimeEmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOvertime(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _overtimeHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Bar chart: hours grouped by project.</summary>
    [HttpGet("charts/hours-by-project")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(ChartResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHoursByProjectChart(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _hoursByProjectChartHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Bar chart: hours grouped by department.</summary>
    [HttpGet("charts/hours-by-department")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(ChartResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHoursByDepartmentChart(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _hoursByDepartmentChartHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Bar chart: hours grouped by employee.</summary>
    [HttpGet("charts/hours-by-employee")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(ChartResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHoursByEmployeeChart(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _hoursByEmployeeChartHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Line chart: monthly hours trend.</summary>
    [HttpGet("charts/hours-by-month")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(ChartResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHoursByMonthChart(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _hoursByMonthChartHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Line chart: daily attendance rate trend.</summary>
    [HttpGet("charts/attendance-trend")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(ChartResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttendanceTrendChart(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _attendanceTrendChartHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Line chart: monthly leave days trend.</summary>
    [HttpGet("charts/leave-trend")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(ChartResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaveTrendChart(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _leaveTrendChartHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    /// <summary>Bar chart: budget used vs remaining per project.</summary>
    [HttpGet("charts/budget-consumption")]
    [RequireAnyPermission(Permissions.AnalyticsReadTeam, Permissions.AnalyticsReadTenant)]
    [ProducesResponseType(typeof(ChartResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBudgetConsumptionChart(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? employeeId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        CancellationToken cancellationToken)
    {
        var featureResult = await EnsureAdvancedReportsEnabledAsync(cancellationToken);
        if (featureResult is not null)
            return featureResult;

        var result = await _budgetConsumptionChartHandler.HandleAsync(
            new AnalyticsQueryParams(departmentId, projectId, employeeId, fromDate, toDate),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
    }

    private async Task<IActionResult?> EnsureAdvancedReportsEnabledAsync(CancellationToken cancellationToken)
    {
        if (await _featureGateService.IsEnabledAsync(FeatureKeys.AdvancedReports, cancellationToken))
            return null;

        return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Plan limit exceeded",
            Detail = "Advanced reports are not available on the current plan. Upgrade to Enterprise."
        });
    }

    private IActionResult MapFailure(HrPortal.SharedKernel.Results.Result result) =>
        result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not found",
                Detail = result.Error
            }),
            "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = result.Error
            }),
            _ => BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad request",
                Detail = result.Error
            })
        };
}
