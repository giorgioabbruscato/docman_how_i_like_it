namespace HrPortal.Notifications;

public sealed record NotificationMessage(
    string Recipient,
    string Subject,
    string Body,
    string Channel = "email");

public interface INotificationService
{
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
}
