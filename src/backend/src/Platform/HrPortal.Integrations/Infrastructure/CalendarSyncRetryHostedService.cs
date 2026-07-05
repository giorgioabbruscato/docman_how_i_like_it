using HrPortal.Integrations.Application;
using HrPortal.Integrations.Domain;
using HrPortal.Integrations.Infrastructure.Persistence;
using HrPortal.SharedKernel.Persistence;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrPortal.Integrations.Infrastructure;

internal sealed class CalendarSyncRetryHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CalendarSyncRetryHostedService> _logger;

    public CalendarSyncRetryHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<CalendarSyncRetryHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAllTenantsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Calendar sync retry job failed");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }

    private async Task ProcessAllTenantsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var tenantRepository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var tenants = await tenantRepository.GetAllAsync(cancellationToken);

        foreach (var tenant in tenants)
        {
            using var tenantScope = _scopeFactory.CreateScope();
            var accessor = tenantScope.ServiceProvider.GetRequiredService<ITenantContextAccessor>();
            accessor.Set(TenantScopingContext.ForSeeding(tenant.Id));

            await RetryPendingSyncsAsync(tenantScope.ServiceProvider, cancellationToken);
        }
    }

    private async Task RetryPendingSyncsAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var syncLogRepository = services.GetRequiredService<ICalendarSyncLogRepository>();
        var syncService = services.GetRequiredService<ICalendarSyncService>();
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();

        var pending = await syncLogRepository.GetPendingRetriesAsync(DateTime.UtcNow, cancellationToken);
        foreach (var log in pending)
        {
            var result = await syncService.SyncLeaveRequestAsync(log.LeaveRequestId, cancellationToken);
            if (result.IsSuccess)
            {
                log.MarkSuccess("Retry succeeded.");
            }
            else
            {
                var delayMinutes = Math.Min(60, (int)Math.Pow(2, log.RetryCount));
                log.MarkRetried(
                    result.Error ?? "Retry failed.",
                    log.RetryCount + 1,
                    DateTime.UtcNow.AddMinutes(delayMinutes));
            }

            await syncLogRepository.UpdateAsync(log, cancellationToken);
        }

        if (pending.Count > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
