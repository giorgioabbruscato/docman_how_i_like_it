using HrPortal.TimeTracking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.TimeTracking.Infrastructure.Persistence;

internal sealed class TimesheetSubmissionConfiguration : IEntityTypeConfiguration<TimesheetSubmission>
{
    public void Configure(EntityTypeBuilder<TimesheetSubmission> builder)
    {
        builder.ToTable("timesheet_submissions", "time_tracking");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Notes)
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasMany(t => t.Entries)
            .WithOne()
            .HasForeignKey(e => e.TimesheetSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Entries)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(t => new { t.TenantId, t.EmployeeId });
        builder.HasIndex(t => new { t.TenantId, t.EmployeeId, t.PeriodStart, t.PeriodEnd });
        builder.HasIndex(t => new { t.TenantId, t.Status });
    }
}

internal sealed class TimesheetSubmissionEntryConfiguration : IEntityTypeConfiguration<TimesheetSubmissionEntry>
{
    public void Configure(EntityTypeBuilder<TimesheetSubmissionEntry> builder)
    {
        builder.ToTable("timesheet_submission_entries", "time_tracking");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.TenantId, e.TimesheetSubmissionId });
        builder.HasIndex(e => new { e.TenantId, e.TimeEntryId });
    }
}

internal sealed class TimesheetApprovalConfiguration : IEntityTypeConfiguration<TimesheetApproval>
{
    public void Configure(EntityTypeBuilder<TimesheetApproval> builder)
    {
        builder.ToTable("timesheet_approvals", "time_tracking");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Decision)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(a => a.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(a => new { a.TenantId, a.TimesheetSubmissionId });
    }
}
