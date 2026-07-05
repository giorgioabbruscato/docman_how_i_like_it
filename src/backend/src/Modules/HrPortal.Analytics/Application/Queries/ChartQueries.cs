using HrPortal.Analytics.Application.Dtos;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Analytics.Application.Queries;

public sealed class GetHoursByProjectChartQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetHoursByProjectChartQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<ChartResponseDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(filterResult.Error!, filterResult.ErrorCode);

        var hoursResult = await _kpiService.GetHoursPerProjectAsync(filterResult.Value!, cancellationToken);
        if (!hoursResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(hoursResult.Error!, hoursResult.ErrorCode);

        return Result.Success(ChartMapping.ToBarChart(hoursResult.Value!, "Hours"));
    }
}

public sealed class GetHoursByDepartmentChartQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetHoursByDepartmentChartQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<ChartResponseDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(filterResult.Error!, filterResult.ErrorCode);

        var hoursResult = await _kpiService.GetHoursPerDepartmentAsync(filterResult.Value!, cancellationToken);
        if (!hoursResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(hoursResult.Error!, hoursResult.ErrorCode);

        return Result.Success(ChartMapping.ToBarChart(hoursResult.Value!, "Hours"));
    }
}

public sealed class GetHoursByEmployeeChartQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetHoursByEmployeeChartQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<ChartResponseDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(filterResult.Error!, filterResult.ErrorCode);

        var hoursResult = await _kpiService.GetHoursPerEmployeeAsync(filterResult.Value!, cancellationToken);
        if (!hoursResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(hoursResult.Error!, hoursResult.ErrorCode);

        return Result.Success(ChartMapping.ToBarChart(hoursResult.Value!, "Hours"));
    }
}

public sealed class GetHoursByMonthChartQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetHoursByMonthChartQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<ChartResponseDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(filterResult.Error!, filterResult.ErrorCode);

        var trendResult = await _kpiService.GetMonthlyTrendAsync(filterResult.Value!, cancellationToken);
        if (!trendResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(trendResult.Error!, trendResult.ErrorCode);

        return Result.Success(ChartMapping.ToLineChartFromMonths(trendResult.Value!, "Hours"));
    }
}

public sealed class GetAttendanceTrendChartQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetAttendanceTrendChartQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<ChartResponseDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(filterResult.Error!, filterResult.ErrorCode);

        var trendResult = await _kpiService.GetDailyAttendanceTrendAsync(filterResult.Value!, cancellationToken);
        if (!trendResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(trendResult.Error!, trendResult.ErrorCode);

        return Result.Success(ChartMapping.ToLineChartFromDates(trendResult.Value!, "Attendance rate"));
    }
}

public sealed class GetLeaveTrendChartQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetLeaveTrendChartQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<ChartResponseDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(filterResult.Error!, filterResult.ErrorCode);

        var trendResult = await _kpiService.GetMonthlyLeaveTrendAsync(filterResult.Value!, cancellationToken);
        if (!trendResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(trendResult.Error!, trendResult.ErrorCode);

        return Result.Success(ChartMapping.ToLineChartFromMonths(trendResult.Value!, "Leave days"));
    }
}

public sealed class GetBudgetConsumptionChartQueryHandler
{
    private readonly IAnalyticsKpiService _kpiService;

    public GetBudgetConsumptionChartQueryHandler(IAnalyticsKpiService kpiService) =>
        _kpiService = kpiService;

    public async Task<Result<ChartResponseDto>> HandleAsync(
        AnalyticsQueryParams query,
        CancellationToken cancellationToken = default)
    {
        var filterResult = await _kpiService.BuildFilterAsync(query, cancellationToken);
        if (!filterResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(filterResult.Error!, filterResult.ErrorCode);

        var budgetResult = await _kpiService.GetBudgetUsageAsync(filterResult.Value!, cancellationToken);
        if (!budgetResult.IsSuccess)
            return Result.Failure<ChartResponseDto>(budgetResult.Error!, budgetResult.ErrorCode);

        return Result.Success(ChartMapping.ToBudgetConsumptionChart(budgetResult.Value!));
    }
}
