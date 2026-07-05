using HrPortal.Integrations.Application;
using HrPortal.Integrations.Infrastructure;
using HrPortal.Integrations.Infrastructure.Persistence;
using HrPortal.Leave.Application;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace HrPortal.Integrations;

public static class IntegrationsServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalIntegrations(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<IntegrationsOptions>(configuration.GetSection(IntegrationsOptions.SectionName));

        services.AddDataProtection();

        services.AddScoped<IOAuthTokenStore, OAuthTokenStore>();
        services.AddScoped<CalendarOAuthStateProtector>();
        services.AddScoped<CalendarTokenService>();
        services.AddScoped<CalendarSyncProviderResolver>();

        services.AddScoped<ICalendarConnectionRepository, CalendarConnectionRepository>();
        services.AddScoped<IExternalCalendarEventRepository, ExternalCalendarEventRepository>();
        services.AddScoped<ICalendarSyncLogRepository, CalendarSyncLogRepository>();

        services.AddScoped<ICalendarConnectionService, CalendarConnectionService>();
        services.AddScoped<ICalendarSyncService, CalendarSyncService>();
        services.RemoveAll<ILeaveCalendarSyncService>();
        services.AddScoped<ILeaveCalendarSyncService, LeaveCalendarSyncService>();

        var options = configuration.GetSection(IntegrationsOptions.SectionName).Get<IntegrationsOptions>()
            ?? new IntegrationsOptions();
        var useMock = options.UseMockProviders || environment.IsEnvironment("Testing");

        if (useMock)
        {
            services.AddSingleton<ICalendarSyncProvider, MockCalendarSyncProvider>();
            services.AddSingleton<ICalendarSyncProvider, MockMicrosoftCalendarSyncProvider>();
        }
        else
        {
            services.AddSingleton<ICalendarSyncProvider>(sp =>
            {
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IntegrationsOptions>>();
                return new GoogleCalendarSyncProvider(new HttpClient(), opts);
            });
            services.AddSingleton<ICalendarSyncProvider>(sp =>
            {
                var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IntegrationsOptions>>();
                return new Microsoft365CalendarSyncProvider(new HttpClient(), opts);
            });
        }

        services.AddHostedService<CalendarSyncRetryHostedService>();

        return services;
    }
}
