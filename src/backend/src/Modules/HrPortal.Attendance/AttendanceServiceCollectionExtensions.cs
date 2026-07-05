using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Commands;
using HrPortal.Attendance.Application.Queries;
using HrPortal.Attendance.Application.Validators;
using HrPortal.Attendance.Infrastructure.Analytics;
using HrPortal.Attendance.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Attendance;

public static class AttendanceServiceCollectionExtensions
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services)
    {
        services.AddScoped<IAttendanceSessionRepository, AttendanceSessionRepository>();
        services.AddScoped<IAttendanceAnalyticsProvider, AttendanceAnalyticsProvider>();

        services.AddScoped<CheckInCommandHandler>();
        services.AddScoped<CheckOutCommandHandler>();
        services.AddScoped<GetAttendanceDashboardQueryHandler>();
        services.AddScoped<GetAttendanceHistoryQueryHandler>();

        services.AddValidatorsFromAssemblyContaining<CheckInRequestValidator>();
        return services;
    }
}
