using HrPortal.Notifications.Application;
using HrPortal.Notifications.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.Notifications;

public static class NotificationsServiceCollectionExtensions
{
    public static IServiceCollection AddHrPortalNotifications(this IServiceCollection services)
    {
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        services.AddScoped<INotificationInboxService, NotificationInboxService>();
        services.AddScoped<INotificationService, LoggingNotificationService>();
        return services;
    }
}
