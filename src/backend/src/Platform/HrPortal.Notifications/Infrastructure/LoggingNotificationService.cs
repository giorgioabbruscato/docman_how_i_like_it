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

    public Task NotifyProjectAssignedAsync(
        Guid recipientUserId,
        string projectName,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "project.assigned",
            recipientUserId,
            "Project assignment",
            $"You have been assigned to project '{projectName}'.",
            $"{{\"projectName\":\"{projectName}\"}}"),
            cancellationToken);

    public Task NotifyTaskAssignedAsync(
        Guid recipientUserId,
        string taskTitle,
        string projectName,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "task.assigned",
            recipientUserId,
            "Task assignment",
            $"You have been assigned to task '{taskTitle}' on project '{projectName}'.",
            $"{{\"taskTitle\":\"{taskTitle}\",\"projectName\":\"{projectName}\"}}"),
            cancellationToken);

    public Task NotifyLeaveApprovedAsync(
        Guid recipientUserId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "leave.approved",
            recipientUserId,
            "Leave request approved",
            $"Your leave request from {startDate} to {endDate} has been approved.",
            $"{{\"startDate\":\"{startDate:yyyy-MM-dd}\",\"endDate\":\"{endDate:yyyy-MM-dd}\"}}"),
            cancellationToken);

    public Task NotifyDocumentUploadedAsync(
        Guid recipientUserId,
        string fileName,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "document.uploaded",
            recipientUserId,
            "Document uploaded",
            $"A document '{fileName}' has been uploaded to your profile.",
            $"{{\"fileName\":\"{fileName}\"}}"),
            cancellationToken);

    public Task NotifyForgottenCheckInAsync(
        Guid recipientUserId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "attendance.forgotten_check_in",
            recipientUserId,
            "Check-in reminder",
            $"You have not checked in for {date:yyyy-MM-dd}.",
            $"{{\"date\":\"{date:yyyy-MM-dd}\"}}"),
            cancellationToken);

    public Task NotifyForgottenCheckOutAsync(
        Guid recipientUserId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "attendance.forgotten_check_out",
            recipientUserId,
            "Check-out reminder",
            $"You still have an open attendance session for {date:yyyy-MM-dd}.",
            $"{{\"date\":\"{date:yyyy-MM-dd}\"}}"),
            cancellationToken);

    private Task DispatchAsync(NotificationPayload payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Notification dispatched: Type={Type}, RecipientUserId={RecipientUserId}, Title={Title}, Metadata={Metadata}",
            payload.Type,
            payload.RecipientUserId,
            payload.Title,
            payload.MetadataJson);

        return Task.CompletedTask;
    }
}
