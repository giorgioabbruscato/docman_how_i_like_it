using HrPortal.Audit.Domain;
using HrPortal.SharedKernel.Entities;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.Persistence;

public sealed class HrPortalDbContext : DbContext
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public HrPortalDbContext(
        DbContextOptions<HrPortalDbContext> options,
        ITenantContextAccessor tenantContextAccessor)
        : base(options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortalDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Tenancy.Domain.Tenant).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditLog).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Departments.Domain.Department).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Employees.Domain.Employee).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Projects.Domain.Project).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Tasks.Domain.ProjectTask).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.TimeTracking.Domain.TimeEntry).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Leave.Domain.LeaveRequest).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Attendance.Domain.AttendanceSession).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Calendar.Domain.PublicHoliday).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Notifications.Domain.UserNotification).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Documents.Domain.Document).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.AccessControl.Domain.TenantRole).Assembly);

        ApplyTenantFilters(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnforceAuditLogImmutability();
        ApplyTenantIdOnInsert();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnforceAuditLogImmutability()
    {
        foreach (var entry in ChangeTracker.Entries<AuditLog>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException("Audit logs are immutable.");
        }
    }

    private void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(HrPortalDbContext)
                .GetMethod(nameof(SetTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !TenantScopingRules.ShouldApplyTenantFilter(_tenantContextAccessor.Current) ||
            e.TenantId == _tenantContextAccessor.Current.TenantId);
    }

    private void ApplyTenantIdOnInsert()
    {
        var tenantContext = _tenantContextAccessor.Current;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State != EntityState.Added || entry.Entity.TenantId != Guid.Empty)
                continue;

            if (tenantContext.Mode == TenantDeploymentMode.Single)
            {
                entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = tenantContext.TenantId;
            }
            else if (tenantContext.IsResolved)
            {
                entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = tenantContext.TenantId;
            }
        }
    }
}
