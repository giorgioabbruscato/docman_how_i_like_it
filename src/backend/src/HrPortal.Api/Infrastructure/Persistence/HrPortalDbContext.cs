using HrPortal.SharedKernel.Entities;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace HrPortal.Api.Infrastructure.Persistence;

public sealed class HrPortalDbContext : DbContext
{
    private readonly TenantContext _tenantContext;

    public HrPortalDbContext(DbContextOptions<HrPortalDbContext> options, TenantContext tenantContext)
        : base(options) => _tenantContext = tenantContext;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortalDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Tenancy.Domain.Tenant).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Audit.Domain.AuditLog).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Departments.Domain.Department).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Employees.Domain.Employee).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Leave.Domain.LeaveRequest).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Attendance.Domain.AttendanceRecord).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrPortal.Documents.Domain.Document).Assembly);

        ApplyTenantFilters(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantIdOnInsert();
        return base.SaveChangesAsync(cancellationToken);
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
            !_tenantContext.IsResolved || e.TenantId == _tenantContext.TenantId);
    }

    private void ApplyTenantIdOnInsert()
    {
        if (!_tenantContext.IsResolved)
            return;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = _tenantContext.TenantId;
            }
        }
    }
}
