using HrPortal.Notifications.Application;
using HrPortal.Notifications.Application.Dtos;
using HrPortal.Notifications.Domain;
using HrPortal.SharedKernel.Persistence;
using HrPortal.SharedKernel.Results;
using HrPortal.Tenancy;

namespace HrPortal.Notifications.Infrastructure;

internal sealed class NotificationInboxService : INotificationInboxService
{
    private readonly IUserNotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TenantContext _tenantContext;

    public NotificationInboxService(
        IUserNotificationRepository repository,
        IUnitOfWork unitOfWork,
        TenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PagedResult<UserNotificationDto>>> GetNotificationsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.UserId.HasValue)
            return Result.Failure<PagedResult<UserNotificationDto>>("User context is required.", "FORBIDDEN");

        var pageResult = await _repository.GetPagedForUserAsync(
            _tenantContext.UserId.Value, page, pageSize, cancellationToken);
        return Result.Success(pageResult);
    }

    public async Task<Result<UserNotificationDto>> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.UserId.HasValue)
            return Result.Failure<UserNotificationDto>("User context is required.", "FORBIDDEN");

        var existing = await _repository.GetByIdForUserAsync(id, _tenantContext.UserId.Value, cancellationToken);
        if (existing is null)
            return Result.Failure<UserNotificationDto>("Notification not found.", "NOT_FOUND");

        var entity = await GetEntityAsync(id, cancellationToken);
        if (entity is null)
            return Result.Failure<UserNotificationDto>("Notification not found.", "NOT_FOUND");

        entity.MarkRead();
        await _repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UserNotificationDto(
            entity.Id, entity.Type, entity.Title, entity.Body,
            entity.MetadataJson, entity.IsRead, entity.CreatedAt));
    }

    private async Task<UserNotification?> GetEntityAsync(Guid id, CancellationToken cancellationToken)
    {
        // Repository only exposes DTOs; load via same DbContext pattern in a dedicated method if needed.
        // For simplicity, re-fetch through a scoped lookup — use repository extension.
        return await _repository.GetEntityByIdForUserAsync(id, _tenantContext.UserId!.Value, cancellationToken);
    }
}
