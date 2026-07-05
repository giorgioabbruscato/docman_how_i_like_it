using HrPortal.SharedKernel.Entities;

namespace HrPortal.Notifications.Domain;

public sealed class UserNotification : AuditableEntity
{
    public Guid RecipientUserId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? MetadataJson { get; private set; }
    public bool IsRead { get; private set; }

    private UserNotification() { }

    public static UserNotification Create(
        Guid tenantId,
        Guid recipientUserId,
        string type,
        string title,
        string body,
        string? metadataJson = null)
    {
        return new UserNotification
        {
            RecipientUserId = recipientUserId,
            Type = type,
            Title = title,
            Body = body,
            MetadataJson = metadataJson,
            IsRead = false
        }.Also(n =>
        {
            n.SetTenant(tenantId);
        });
    }

    public void MarkRead()
    {
        IsRead = true;
        MarkUpdated(null);
    }
}

internal static class UserNotificationExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
