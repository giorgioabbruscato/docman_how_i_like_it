using HrPortal.Attendance.Application;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HrPortal.Attendance.Infrastructure;

internal sealed class AttendanceReminderHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AttendanceReminderHostedService> _logger;

    public AttendanceReminderHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AttendanceReminderHostedService> logger)
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
                _logger.LogError(ex, "Attendance reminder job failed");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
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

            var reminderService = tenantScope.ServiceProvider.GetRequiredService<IAttendanceReminderService>();
            await reminderService.ProcessRemindersAsync(DateTime.UtcNow, cancellationToken);
        }
    }
}
