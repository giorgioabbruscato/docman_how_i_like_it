using Microsoft.Extensions.Logging;

namespace HrPortal.Notifications;

public static class NotificationHelper
{
    public static async Task TryNotifyAsync(
        ILogger logger,
        Func<CancellationToken, Task> notify,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await notify(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Notification delivery failed");
        }
    }

    public static void FireAndForget(
        ILogger logger,
        Func<CancellationToken, Task> notify)
    {
        _ = Task.Run(async () => await TryNotifyAsync(logger, notify, CancellationToken.None));
    }
}
