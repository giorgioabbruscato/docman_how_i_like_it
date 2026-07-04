using HrPortal.Notifications.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Notifications;

public static class NotificationsServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalNotifications(this IServiceCollection services)
    {
        services.AddSingleton<INotificationService, LoggingNotificationService>();
        return services;
    }
}
