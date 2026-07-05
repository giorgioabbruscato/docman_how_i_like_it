using HrPortal.Leave.Application;
using HrPortal.Leave.Application.Validators;
using HrPortal.Leave.Infrastructure.Analytics;
using HrPortal.Leave.Infrastructure.Calendar;
using HrPortal.Leave.Infrastructure.Persistence;
using HrPortal.Calendar.Application;
using FluentValidation;
using HrPortal.Leave.Infrastructure.Workflows;
using HrPortal.Workflows.Application;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Leave;

public static class LeaveServiceCollectionExtensions
{
    public static IServiceCollection AddLeaveModule(this IServiceCollection services)
    {
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<ILeaveAnalyticsProvider, LeaveAnalyticsProvider>();
        services.AddScoped<ILeaveCalendarProvider, LeaveCalendarProvider>();
        services.AddScoped<IWorkflowCompletionHandler, LeaveWorkflowCompletionHandler>();
        services.AddValidatorsFromAssemblyContaining<CreateLeaveRequestValidator>();
        return services;
    }
}
