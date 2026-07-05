namespace HrPortal.AccessControl.Application;

public sealed record NotificationRecipient(Guid? UserId, string LogIdentifier);

public interface INotificationRecipientResolver
{
    Task<NotificationRecipient> ResolveForEmployeeAsync(
        Guid employeeId,
        string fallbackEmail,
        CancellationToken cancellationToken = default);
}
