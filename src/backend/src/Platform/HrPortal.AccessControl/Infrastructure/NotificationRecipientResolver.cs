using HrPortal.AccessControl.Application;

namespace HrPortal.AccessControl.Infrastructure;

internal sealed class NotificationRecipientResolver : INotificationRecipientResolver
{
    private readonly ITenantMembershipRepository _membershipRepository;

    public NotificationRecipientResolver(ITenantMembershipRepository membershipRepository) =>
        _membershipRepository = membershipRepository;

    public async Task<NotificationRecipient> ResolveForEmployeeAsync(
        Guid employeeId,
        string fallbackEmail,
        CancellationToken cancellationToken = default)
    {
        var membership = await _membershipRepository.GetActiveByEmployeeIdAsync(employeeId, cancellationToken);

        return membership is null
            ? new NotificationRecipient(null, fallbackEmail)
            : new NotificationRecipient(membership.UserId, membership.UserId.ToString());
    }
}
