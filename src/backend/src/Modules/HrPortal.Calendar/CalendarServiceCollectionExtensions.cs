using HrPortal.Calendar.Application;
using HrPortal.Calendar.Infrastructure;
using HrPortal.Calendar.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Calendar;

public static class CalendarServiceCollectionExtensions
{
    public static IServiceCollection AddCalendarModule(this IServiceCollection services)
    {
        services.AddScoped<IPublicHolidayRepository, PublicHolidayRepository>();
        services.AddScoped<ISmartWorkingScheduleRepository, SmartWorkingScheduleRepository>();
        services.AddScoped<ISmartWorkingCalendarProvider, SmartWorkingCalendarProvider>();
        services.AddScoped<ICalendarQueryService, CalendarQueryService>();
        services.AddScoped<IPublicHolidayService, PublicHolidayService>();
        return services;
    }
}
