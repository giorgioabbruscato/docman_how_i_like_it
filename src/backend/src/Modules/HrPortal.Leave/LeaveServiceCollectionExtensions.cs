using HrPortal.Leave.Application;
using HrPortal.Leave.Application.Validators;
using HrPortal.Leave.Infrastructure.Analytics;
using HrPortal.Leave.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Leave;

public static class LeaveServiceCollectionExtensions
{
    public static IServiceCollection AddLeaveModule(this IServiceCollection services)
    {
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<ILeaveAnalyticsProvider, LeaveAnalyticsProvider>();
        services.AddValidatorsFromAssemblyContaining<CreateLeaveRequestValidator>();
        return services;
    }
}
