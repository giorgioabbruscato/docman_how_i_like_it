using HrPortal.SharedKernel.Entities;

namespace HrPortal.Integrations.Domain;

public sealed class CalendarConnection : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public CalendarProvider Provider { get; private set; }
    public string? ExternalCalendarId { get; private set; }
    public string AccessTokenEncrypted { get; private set; } = string.Empty;
    public string? RefreshTokenEncrypted { get; private set; }
    public DateTime? TokenExpiresAt { get; private set; }
    public DateTime ConnectedAt { get; private set; }
    public bool IsActive { get; private set; }

    private CalendarConnection() { }

    public static CalendarConnection Create(
        Guid tenantId,
        Guid employeeId,
        CalendarProvider provider,
        string accessTokenEncrypted,
        string? refreshTokenEncrypted,
        DateTime? tokenExpiresAt,
        string? externalCalendarId = null)
    {
        return new CalendarConnection
        {
            EmployeeId = employeeId,
            Provider = provider,
            AccessTokenEncrypted = accessTokenEncrypted,
            RefreshTokenEncrypted = refreshTokenEncrypted,
            TokenExpiresAt = tokenExpiresAt,
            ExternalCalendarId = externalCalendarId,
            ConnectedAt = DateTime.UtcNow,
            IsActive = true
        }.Also(c => c.SetTenant(tenantId));
    }

    public void UpdateTokens(string accessTokenEncrypted, string? refreshTokenEncrypted, DateTime? tokenExpiresAt)
    {
        AccessTokenEncrypted = accessTokenEncrypted;
        RefreshTokenEncrypted = refreshTokenEncrypted;
        TokenExpiresAt = tokenExpiresAt;
        MarkUpdated(null);
    }

    public void Deactivate()
    {
        IsActive = false;
        AccessTokenEncrypted = string.Empty;
        RefreshTokenEncrypted = null;
        TokenExpiresAt = null;
        MarkUpdated(null);
    }

    public void Reconnect(string accessTokenEncrypted, string? refreshTokenEncrypted, DateTime? tokenExpiresAt)
    {
        IsActive = true;
        AccessTokenEncrypted = accessTokenEncrypted;
        RefreshTokenEncrypted = refreshTokenEncrypted;
        TokenExpiresAt = tokenExpiresAt;
        ConnectedAt = DateTime.UtcNow;
        MarkUpdated(null);
    }
}

internal static class CalendarConnectionExtensions
{
    public static T Also<T>(this T obj, Action<T> action)
    {
        action(obj);
        return obj;
    }
}
