using HrPortal.Notifications.Application;
using HrPortal.Notifications.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using Microsoft.Extensions.Logging;

namespace HrPortal.Notifications.Infrastructure;

internal sealed class LoggingNotificationService : INotificationService
{
    private readonly IUserNotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;
    private readonly ILogger<LoggingNotificationService> _logger;

    public LoggingNotificationService(
        IUserNotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext,
        ILogger<LoggingNotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

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

    public Task NotifyTimesheetSubmittedAsync(
        Guid recipientUserId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "timesheet.submitted",
            recipientUserId,
            "Timesheet submitted",
            $"A timesheet for {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd} awaits your approval.",
            $"{{\"periodStart\":\"{periodStart:yyyy-MM-dd}\",\"periodEnd\":\"{periodEnd:yyyy-MM-dd}\"}}"),
            cancellationToken);

    public Task NotifyTimesheetApprovedAsync(
        Guid recipientUserId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "timesheet.approved",
            recipientUserId,
            "Timesheet approved",
            $"Your timesheet for {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd} has been approved.",
            $"{{\"periodStart\":\"{periodStart:yyyy-MM-dd}\",\"periodEnd\":\"{periodEnd:yyyy-MM-dd}\"}}"),
            cancellationToken);

    public Task NotifyTimesheetRejectedAsync(
        Guid recipientUserId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(new NotificationPayload(
            "timesheet.rejected",
            recipientUserId,
            "Timesheet rejected",
            $"Your timesheet for {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd} has been rejected.",
            $"{{\"periodStart\":\"{periodStart:yyyy-MM-dd}\",\"periodEnd\":\"{periodEnd:yyyy-MM-dd}\"}}"),
            cancellationToken);

    private async Task DispatchAsync(NotificationPayload payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Notification dispatched: Type={Type}, RecipientUserId={RecipientUserId}, Title={Title}, Metadata={Metadata}",
            payload.Type,
            payload.RecipientUserId,
            payload.Title,
            payload.MetadataJson);

        var notification = UserNotification.Create(
            _tenantContext.TenantId,
            payload.RecipientUserId,
            payload.Type,
            payload.Title,
            payload.Body,
            payload.MetadataJson);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
