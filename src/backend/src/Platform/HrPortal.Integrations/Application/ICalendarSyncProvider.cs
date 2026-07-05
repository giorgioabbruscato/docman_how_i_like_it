using HrPortal.Integrations.Domain;

namespace HrPortal.Integrations.Application;

public sealed class OAuthTokens
{
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public interface IOAuthTokenStore
{
    string Protect(string plaintext);
    string Unprotect(string protectedData);
}

public interface ICalendarSyncProvider
{
    CalendarProvider Provider { get; }

    string GetAuthorizationUrl(string state, string redirectUri);

    Task<OAuthTokens> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default);

    Task<OAuthTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<string> CreateOrUpdateEventAsync(
        CalendarConnectionContext connection,
        LeaveEventContext leave,
        string? existingExternalEventId,
        CancellationToken cancellationToken = default);

    Task DeleteEventAsync(
        CalendarConnectionContext connection,
        string externalEventId,
        CancellationToken cancellationToken = default);
}

public sealed class CalendarConnectionContext
{
    public required Guid ConnectionId { get; init; }
    public required string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime? TokenExpiresAt { get; init; }
    public string? ExternalCalendarId { get; init; }
}

public sealed class LeaveEventContext
{
    public required Guid LeaveRequestId { get; init; }
    public required Guid EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required string LeaveType { get; init; }
    public string? Reason { get; init; }
}
