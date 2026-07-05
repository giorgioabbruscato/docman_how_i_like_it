using HrPortal.Notifications.Application.Dtos;
using HrPortal.Notifications.Domain;
using HrPortal.SharedKernel.Results;

namespace HrPortal.Notifications.Application;

public interface IUserNotificationRepository
{
    Task<PagedResult<UserNotificationDto>> GetPagedForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<UserNotificationDto?> GetByIdForUserAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);
    Task<UserNotification?> GetEntityByIdForUserAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);
    Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserNotification notification, CancellationToken cancellationToken = default);
}

public interface INotificationInboxService
{
    Task<Result<PagedResult<UserNotificationDto>>> GetNotificationsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Result<UserNotificationDto>> MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);
}
