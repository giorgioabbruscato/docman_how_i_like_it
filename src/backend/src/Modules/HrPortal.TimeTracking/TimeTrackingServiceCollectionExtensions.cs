using FluentValidation;
using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Commands;
using HrPortal.TimeTracking.Application.Queries;
using HrPortal.TimeTracking.Application.Validators;
using HrPortal.TimeTracking.Infrastructure.Analytics;
using HrPortal.TimeTracking.Infrastructure.Export;
using HrPortal.TimeTracking.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.TimeTracking;

public static class TimeTrackingServiceCollectionExtensions
{
    public static IServiceCollection AddTimeTrackingModule(this IServiceCollection services)
    {
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<ITimeEntryExportService, TimeEntryExportService>();
        services.AddScoped<ITimeEntryAnalyticsProvider, TimeEntryAnalyticsProvider>();
        services.AddScoped<ITimesheetApprovalService, TimesheetApprovalService>();

        services.AddScoped<CreateTimeEntryCommandHandler>();
        services.AddScoped<UpdateTimeEntryCommandHandler>();
        services.AddScoped<DeleteTimeEntryCommandHandler>();
        services.AddScoped<StartTimerCommandHandler>();
        services.AddScoped<StopTimerCommandHandler>();
        services.AddScoped<CreateManualTimeEntryCommandHandler>();
        services.AddScoped<CreateTimesheetCommandHandler>();
        services.AddScoped<SubmitTimesheetCommandHandler>();
        services.AddScoped<GetTimeEntriesQueryHandler>();
        services.AddScoped<GetTimeEntryByIdQueryHandler>();
        services.AddScoped<GetActiveTimerQueryHandler>();
        services.AddScoped<ExportTimeEntriesQueryHandler>();
        services.AddScoped<GetTimesheetsQueryHandler>();
        services.AddScoped<GetTimesheetByIdQueryHandler>();

        services.AddValidatorsFromAssemblyContaining<CreateTimeEntryRequestValidator>();
        return services;
    }
}
