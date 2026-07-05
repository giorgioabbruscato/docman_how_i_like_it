using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using HrPortal.Integrations.Application;
using HrPortal.Integrations.Domain;
using Microsoft.Extensions.Options;

namespace HrPortal.Integrations.Infrastructure;

internal sealed class MockCalendarSyncProvider : ICalendarSyncProvider
{
    public CalendarProvider Provider => CalendarProvider.Google;

    public string GetAuthorizationUrl(string state, string redirectUri) =>
        $"{redirectUri}?code=mock-auth-code&state={Uri.EscapeDataString(state)}";

    public Task<OAuthTokens> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new OAuthTokens
        {
            AccessToken = "mock-access-token",
            RefreshToken = "mock-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });

    public Task<OAuthTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(new OAuthTokens
        {
            AccessToken = "mock-refreshed-access-token",
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });

    public Task<string> CreateOrUpdateEventAsync(
        CalendarConnectionContext connection,
        LeaveEventContext leave,
        string? existingExternalEventId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(existingExternalEventId ?? $"mock-event-{leave.LeaveRequestId:N}");

    public Task DeleteEventAsync(
        CalendarConnectionContext connection,
        string externalEventId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

internal sealed class MockMicrosoftCalendarSyncProvider : ICalendarSyncProvider
{
    public CalendarProvider Provider => CalendarProvider.Microsoft365;

    public string GetAuthorizationUrl(string state, string redirectUri) =>
        $"{redirectUri}?code=mock-auth-code&state={Uri.EscapeDataString(state)}";

    public Task<OAuthTokens> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new OAuthTokens
        {
            AccessToken = "mock-ms-access-token",
            RefreshToken = "mock-ms-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });

    public Task<OAuthTokens> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(new OAuthTokens
        {
            AccessToken = "mock-ms-refreshed-access-token",
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });

    public Task<string> CreateOrUpdateEventAsync(
        CalendarConnectionContext connection,
        LeaveEventContext leave,
        string? existingExternalEventId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(existingExternalEventId ?? $"mock-ms-event-{leave.LeaveRequestId:N}");

    public Task DeleteEventAsync(
        CalendarConnectionContext connection,
        string externalEventId,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

internal abstract class HttpCalendarSyncProviderBase : ICalendarSyncProvider
{
    private readonly HttpClient _httpClient;
    private readonly IntegrationsOptions _options;

    protected HttpCalendarSyncProviderBase(HttpClient httpClient, IOptions<IntegrationsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public abstract CalendarProvider Provider { get; }

    public abstract string GetAuthorizationUrl(string state, string redirectUri);

    public abstract Task<OAuthTokens> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default);

    public abstract Task<OAuthTokens> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    public abstract Task<string> CreateOrUpdateEventAsync(
        CalendarConnectionContext connection,
        LeaveEventContext leave,
        string? existingExternalEventId,
        CancellationToken cancellationToken = default);

    public abstract Task DeleteEventAsync(
        CalendarConnectionContext connection,
        string externalEventId,
        CancellationToken cancellationToken = default);

    protected async Task<JsonDocument> SendJsonAsync(
        HttpMethod method,
        string url,
        object? body,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Calendar API call failed ({response.StatusCode}): {content}");

        return string.IsNullOrWhiteSpace(content)
            ? JsonDocument.Parse("{}")
            : JsonDocument.Parse(content);
    }

    protected IntegrationsOptions Options => _options;
}

internal sealed class GoogleCalendarSyncProvider : HttpCalendarSyncProviderBase
{
    private const string AuthEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
    private const string CalendarScope = "https://www.googleapis.com/auth/calendar.events";

    public GoogleCalendarSyncProvider(HttpClient httpClient, IOptions<IntegrationsOptions> options)
        : base(httpClient, options)
    {
    }

    public override CalendarProvider Provider => CalendarProvider.Google;

    public override string GetAuthorizationUrl(string state, string redirectUri)
    {
        var clientId = Options.Google.ClientId;
        var query = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = CalendarScope,
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["state"] = state
        };

        return $"{AuthEndpoint}?{string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))}";
    }

    public override async Task<OAuthTokens> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        var response = await PostTokenAsync(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = Options.Google.ClientId,
            ["client_secret"] = Options.Google.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        }, cancellationToken);

        return ParseTokenResponse(response);
    }

    public override async Task<OAuthTokens> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var response = await PostTokenAsync(new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = Options.Google.ClientId,
            ["client_secret"] = Options.Google.ClientSecret,
            ["grant_type"] = "refresh_token"
        }, cancellationToken);

        return ParseTokenResponse(response);
    }

    public override async Task<string> CreateOrUpdateEventAsync(
        CalendarConnectionContext connection,
        LeaveEventContext leave,
        string? existingExternalEventId,
        CancellationToken cancellationToken = default)
    {
        var calendarId = connection.ExternalCalendarId ?? "primary";
        var eventBody = BuildAllDayEvent(leave);
        var url = existingExternalEventId is null
            ? $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events"
            : $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events/{Uri.EscapeDataString(existingExternalEventId)}";

        using var response = await SendJsonAsync(
            existingExternalEventId is null ? HttpMethod.Post : HttpMethod.Put,
            url,
            eventBody,
            connection.AccessToken,
            cancellationToken);

        return response.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Google Calendar response missing event id.");
    }

    public override async Task DeleteEventAsync(
        CalendarConnectionContext connection,
        string externalEventId,
        CancellationToken cancellationToken = default)
    {
        var calendarId = connection.ExternalCalendarId ?? "primary";
        var url =
            $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events/{Uri.EscapeDataString(externalEventId)}";

        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.AccessToken);
        using var httpClient = new HttpClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Google Calendar delete failed ({response.StatusCode}): {content}");
        }
    }

    private async Task<JsonDocument> PostTokenAsync(
        Dictionary<string, string> form,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(form);
        using var httpClient = new HttpClient();
        using var response = await httpClient.PostAsync(TokenEndpoint, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Google token exchange failed ({response.StatusCode}): {responseContent}");

        return JsonDocument.Parse(responseContent);
    }

    private static object BuildAllDayEvent(LeaveEventContext leave) => new
    {
        summary = $"{leave.LeaveType} leave — {leave.EmployeeName}",
        description = leave.Reason,
        start = new { date = leave.StartDate.ToString("yyyy-MM-dd") },
        end = new { date = leave.EndDate.AddDays(1).ToString("yyyy-MM-dd") }
    };

    private static OAuthTokens ParseTokenResponse(JsonDocument response)
    {
        var root = response.RootElement;
        var expiresIn = root.TryGetProperty("expires_in", out var expires) ? expires.GetInt32() : 3600;

        return new OAuthTokens
        {
            AccessToken = root.GetProperty("access_token").GetString()!,
            RefreshToken = root.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() : null,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn)
        };
    }
}

internal sealed class Microsoft365CalendarSyncProvider : HttpCalendarSyncProviderBase
{
    private const string AuthEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
    private const string TokenEndpoint = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
    private const string CalendarScope = "offline_access Calendars.ReadWrite";

    public Microsoft365CalendarSyncProvider(HttpClient httpClient, IOptions<IntegrationsOptions> options)
        : base(httpClient, options)
    {
    }

    public override CalendarProvider Provider => CalendarProvider.Microsoft365;

    public override string GetAuthorizationUrl(string state, string redirectUri)
    {
        var query = new Dictionary<string, string>
        {
            ["client_id"] = Options.Microsoft.ClientId,
            ["redirect_uri"] = redirectUri,
            ["response_type"] = "code",
            ["scope"] = CalendarScope,
            ["state"] = state
        };

        return $"{AuthEndpoint}?{string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"))}";
    }

    public override async Task<OAuthTokens> ExchangeCodeAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = Options.Microsoft.ClientId,
            ["client_secret"] = Options.Microsoft.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint) { Content = content };
        using var response = await SendTokenRequestAsync(request, cancellationToken);
        return ParseTokenResponse(response);
    }

    public override async Task<OAuthTokens> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["refresh_token"] = refreshToken,
            ["client_id"] = Options.Microsoft.ClientId,
            ["client_secret"] = Options.Microsoft.ClientSecret,
            ["grant_type"] = "refresh_token"
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint) { Content = content };
        using var response = await SendTokenRequestAsync(request, cancellationToken);
        return ParseTokenResponse(response);
    }

    public override async Task<string> CreateOrUpdateEventAsync(
        CalendarConnectionContext connection,
        LeaveEventContext leave,
        string? existingExternalEventId,
        CancellationToken cancellationToken = default)
    {
        var eventBody = new
        {
            subject = $"{leave.LeaveType} leave — {leave.EmployeeName}",
            body = new { contentType = "text", content = leave.Reason ?? string.Empty },
            start = new { dateTime = leave.StartDate.ToString("yyyy-MM-dd"), timeZone = "UTC" },
            end = new { dateTime = leave.EndDate.AddDays(1).ToString("yyyy-MM-dd"), timeZone = "UTC" },
            isAllDay = true
        };

        var url = existingExternalEventId is null
            ? "https://graph.microsoft.com/v1.0/me/events"
            : $"https://graph.microsoft.com/v1.0/me/events/{existingExternalEventId}";

        using var response = await SendJsonAsync(
            existingExternalEventId is null ? HttpMethod.Post : HttpMethod.Patch,
            url,
            eventBody,
            connection.AccessToken,
            cancellationToken);

        return response.RootElement.GetProperty("id").GetString()
            ?? throw new InvalidOperationException("Microsoft Graph response missing event id.");
    }

    public override async Task DeleteEventAsync(
        CalendarConnectionContext connection,
        string externalEventId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://graph.microsoft.com/v1.0/me/events/{externalEventId}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.AccessToken);
        using var httpClient = new HttpClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Microsoft Graph delete failed ({response.StatusCode}): {content}");
        }
    }

    private async Task<JsonDocument> SendTokenRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Microsoft token exchange failed ({response.StatusCode}): {content}");

        return JsonDocument.Parse(content);
    }

    private static OAuthTokens ParseTokenResponse(JsonDocument response)
    {
        var root = response.RootElement;
        var expiresIn = root.TryGetProperty("expires_in", out var expires) ? expires.GetInt32() : 3600;

        return new OAuthTokens
        {
            AccessToken = root.GetProperty("access_token").GetString()!,
            RefreshToken = root.TryGetProperty("refresh_token", out var refresh) ? refresh.GetString() : null,
            ExpiresAt = DateTime.UtcNow.AddSeconds(expiresIn)
        };
    }
}
