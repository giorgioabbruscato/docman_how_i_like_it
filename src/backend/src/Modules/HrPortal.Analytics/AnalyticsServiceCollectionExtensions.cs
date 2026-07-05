using HrPortal.Analytics.Application;
using HrPortal.Analytics.Application.Options;
using HrPortal.Analytics.Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Analytics;

public static class AnalyticsServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsModule(this IServiceCollection services)
    {
        services.Configure<AnalyticsOptions>(options => { });

        services.AddScoped<IAnalyticsKpiService, AnalyticsKpiService>();

        services.AddScoped<GetSupervisorSummaryQueryHandler>();
        services.AddScoped<GetEmployeesWorkingQueryHandler>();
        services.AddScoped<GetAttendanceTodayQueryHandler>();
        services.AddScoped<GetTopEmployeesQueryHandler>();
        services.AddScoped<GetTopProjectsQueryHandler>();
        services.AddScoped<GetBudgetUsageQueryHandler>();
        services.AddScoped<GetLateArrivalsQueryHandler>();
        services.AddScoped<GetOvertimeQueryHandler>();

        services.AddScoped<GetHoursByProjectChartQueryHandler>();
        services.AddScoped<GetHoursByDepartmentChartQueryHandler>();
        services.AddScoped<GetHoursByEmployeeChartQueryHandler>();
        services.AddScoped<GetHoursByMonthChartQueryHandler>();
        services.AddScoped<GetAttendanceTrendChartQueryHandler>();
        services.AddScoped<GetLeaveTrendChartQueryHandler>();
        services.AddScoped<GetBudgetConsumptionChartQueryHandler>();

        return services;
    }
}
