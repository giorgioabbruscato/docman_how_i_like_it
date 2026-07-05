namespace HrPortal.Notifications;

public sealed record NotificationMessage(
    string Recipient,
    string Subject,
    string Body,
    string Channel = "email");

public sealed record NotificationPayload(
    string Type,
    Guid RecipientUserId,
    string Title,
    string Body,
    string? MetadataJson = null);

public interface INotificationService
{
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);

    Task NotifyProjectAssignedAsync(
        Guid recipientUserId,
        string projectName,
        CancellationToken cancellationToken = default);

    Task NotifyTaskAssignedAsync(
        Guid recipientUserId,
        string taskTitle,
        string projectName,
        CancellationToken cancellationToken = default);

    Task NotifyLeaveApprovedAsync(
        Guid recipientUserId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task NotifyDocumentUploadedAsync(
        Guid recipientUserId,
        string fileName,
        CancellationToken cancellationToken = default);

    Task NotifyForgottenCheckInAsync(
        Guid recipientUserId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task NotifyForgottenCheckOutAsync(
        Guid recipientUserId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task NotifyTimesheetSubmittedAsync(
        Guid recipientUserId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);

    Task NotifyTimesheetApprovedAsync(
        Guid recipientUserId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);

    Task NotifyTimesheetRejectedAsync(
        Guid recipientUserId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default);

    Task NotifyWorkflowActionRequiredAsync(
        Guid approverEmployeeId,
        string requestType,
        Guid requestId,
        string stepName,
        CancellationToken cancellationToken = default);
}
