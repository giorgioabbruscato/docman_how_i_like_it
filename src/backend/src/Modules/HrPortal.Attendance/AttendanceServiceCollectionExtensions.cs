using HrPortal.Attendance.Application;
using HrPortal.Attendance.Application.Validators;
using HrPortal.Attendance.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Attendance;

public static class AttendanceServiceCollectionExtensions
{
    public static IServiceCollection AddAttendanceModule(this IServiceCollection services)
    {
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddValidatorsFromAssemblyContaining<CheckInRequestValidator>();
        return services;
    }
}
