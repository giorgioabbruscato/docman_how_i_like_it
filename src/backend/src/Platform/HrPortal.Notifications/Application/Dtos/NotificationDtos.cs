namespace HrPortal.Notifications.Application.Dtos;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record UserNotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? MetadataJson,
    bool IsRead,
    DateTime CreatedAt);
