using HrPortal.Integrations.Domain;

namespace HrPortal.Integrations.Application.Dtos;

public sealed record CalendarProviderDto(string Id, string Name);

public sealed record CalendarConnectResponse(string AuthorizationUrl);

public sealed record CalendarCallbackResult(string RedirectUri, bool Success, string? Error = null);

public sealed record CalendarConnectionDto(
    Guid Id,
    string Provider,
    DateTime ConnectedAt,
    bool IsActive);

public sealed record CalendarSyncLogDto(
    Guid Id,
    Guid LeaveRequestId,
    Guid? EmployeeId,
    string? Provider,
    string Status,
    string? Message,
    int RetryCount,
    DateTime? NextRetryAt,
    DateTime CreatedAt);
