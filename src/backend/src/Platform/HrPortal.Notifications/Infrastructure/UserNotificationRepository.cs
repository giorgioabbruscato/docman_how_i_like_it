using HrPortal.Notifications.Application;
using HrPortal.Notifications.Application.Dtos;
using HrPortal.Notifications.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Notifications.Infrastructure;

internal sealed class UserNotificationRepository : IUserNotificationRepository
{
    private readonly DbContext _dbContext;
    private readonly ITenantContextAccessor _accessor;

    public UserNotificationRepository(DbContext dbContext, ITenantContextAccessor accessor)
    {
        _dbContext = dbContext;
        _accessor = accessor;
    }

    public async Task<PagedResult<UserNotificationDto>> GetPagedForUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<UserNotification>()
            .ApplyTenantScope(_accessor.Current)
            .Where(n => n.RecipientUserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new UserNotificationDto(
                n.Id, n.Type, n.Title, n.Body, n.MetadataJson, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<UserNotificationDto>(items, totalCount, page, pageSize);
    }

    public async Task<UserNotificationDto?> GetByIdForUserAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<UserNotification>()
            .ApplyTenantScope(_accessor.Current)
            .Where(n => n.Id == id && n.RecipientUserId == userId)
            .Select(n => new UserNotificationDto(
                n.Id, n.Type, n.Title, n.Body, n.MetadataJson, n.IsRead, n.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<UserNotification?> GetEntityByIdForUserAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await _dbContext.Set<UserNotification>()
            .ApplyTenantScope(_accessor.Current)
            .FirstOrDefaultAsync(n => n.Id == id && n.RecipientUserId == userId, cancellationToken);

    public async Task AddAsync(UserNotification notification, CancellationToken cancellationToken = default) =>
        await _dbContext.Set<UserNotification>().AddAsync(notification, cancellationToken);

    public Task UpdateAsync(UserNotification notification, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<UserNotification>().Update(notification);
        return Task.CompletedTask;
    }
}
