using HrPortal.Analytics.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Analytics.Application.Queries;

public sealed class GetSupervisorSummaryQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetSupervisorSummaryQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<SupervisorSummaryDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<SupervisorSummaryDto>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetSupervisorSummaryAsync(filterResult.Value!, cancellationToken);
    }
}

public sealed class GetEmployeesWorkingQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetEmployeesWorkingQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<IReadOnlyList<EmployeeWorkingDto>>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<IReadOnlyList<EmployeeWorkingDto>>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetEmployeesWorkingAsync(filterResult.Value!, cancellationToken);
    }
}

public sealed class GetAttendanceTodayQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetAttendanceTodayQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<IReadOnlyList<AttendanceTodayDto>>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<IReadOnlyList<AttendanceTodayDto>>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetAttendanceTodayAsync(filterResult.Value!, cancellationToken);
    }
}

public sealed class GetTopEmployeesQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetTopEmployeesQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<IReadOnlyList<TopEmployeeDto>>> HandleAsync(
        AnalyticsQueryParams query,
        int top = 5,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<IReadOnlyList<TopEmployeeDto>>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetTopEmployeesAsync(filterResult.Value!, top, cancellationToken);
    }
}

public sealed class GetTopProjectsQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetTopProjectsQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<IReadOnlyList<TopProjectDto>>> HandleAsync(
        AnalyticsQueryParams query,
        int top = 5,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<IReadOnlyList<TopProjectDto>>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetTopProjectsAsync(filterResult.Value!, top, cancellationToken);
    }
}

public sealed class GetBudgetUsageQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetBudgetUsageQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<IReadOnlyList<BudgetUsageDto>>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<IReadOnlyList<BudgetUsageDto>>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetBudgetUsageAsync(filterResult.Value!, cancellationToken);
    }
}

public sealed class GetLateArrivalsQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetLateArrivalsQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<IReadOnlyList<LateArrivalDto>>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<IReadOnlyList<LateArrivalDto>>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetLateArrivalsTodayAsync(filterResult.Value!, cancellationToken);
    }
}

public sealed class GetOvertimeQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetOvertimeQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<IReadOnlyList<OvertimeEmployeeDto>>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<IReadOnlyList<OvertimeEmployeeDto>>(filterResult.Error!, filterResult.ErrorCode);

        return await _kpiService.GetOvertimeByEmployeeAsync(filterResult.Value!, cancellationToken);
    }
}
