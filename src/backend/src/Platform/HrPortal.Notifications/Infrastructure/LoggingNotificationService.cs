using Microsoft.Extensions.Logging;

namespace HrPortal.Notifications.Infrastructure;

internal sealed class LoggingNotificationService : INotificationService
{
    private readonly ILogger<LoggingNotificationService> _logger;

    public LoggingNotificationService(ILogger<LoggingNotificationService> logger) =>
        _logger = logger;

    public Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Notification queued: Channel={Channel}, Recipient={Recipient}, Subject={Subject}",
            message.Channel,
            message.Recipient,
            message.Subject);

        return Task.CompletedTask;
    }
}
