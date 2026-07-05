using HrPortal.TimeTracking.Application;
using HrPortal.TimeTracking.Application.Dtos;
using HrPortal.TimeTracking.Domain;
using HrPortal.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.TimeTracking.Infrastructure.Persistence;

internal sealed class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("time_entries", "time_tracking");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.WorkedMinutes)
            .IsRequired();

        builder.Property(t => t.Billable)
            .IsRequired();

        builder.HasIndex(t => new { t.TenantId, t.EmployeeId });
        builder.HasIndex(t => new { t.TenantId, t.EmployeeId, t.StartTime });
        builder.HasIndex(t => new { t.TenantId, t.ProjectId });
    }
}
