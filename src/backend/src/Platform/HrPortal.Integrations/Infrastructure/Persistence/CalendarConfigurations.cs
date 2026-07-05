using HrPortal.Integrations.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Integrations.Infrastructure.Persistence;

internal sealed class CalendarConnectionConfiguration : IEntityTypeConfiguration<CalendarConnection>
{
    public void Configure(EntityTypeBuilder<CalendarConnection> builder)
    {
        builder.ToTable("calendar_connections", "integrations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Provider).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(c => c.ExternalCalendarId).HasMaxLength(500);
        builder.Property(c => c.AccessTokenEncrypted).HasMaxLength(8000).IsRequired();
        builder.Property(c => c.RefreshTokenEncrypted).HasMaxLength(8000);

        builder.HasIndex(c => new { c.TenantId, c.EmployeeId, c.Provider });
        builder.HasIndex(c => new { c.TenantId, c.EmployeeId, c.IsActive });
    }
}

internal sealed class ExternalCalendarEventConfiguration : IEntityTypeConfiguration<ExternalCalendarEvent>
{
    public void Configure(EntityTypeBuilder<ExternalCalendarEvent> builder)
    {
        builder.ToTable("external_calendar_events", "integrations");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Provider).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.ExternalEventId).HasMaxLength(500).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.LeaveRequestId, e.Provider }).IsUnique();
    }
}

internal sealed class CalendarSyncLogConfiguration : IEntityTypeConfiguration<CalendarSyncLog>
{
    public void Configure(EntityTypeBuilder<CalendarSyncLog> builder)
    {
        builder.ToTable("calendar_sync_logs", "integrations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Provider).HasConversion<string>().HasMaxLength(50);
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(l => l.Message).HasMaxLength(2000);

        builder.HasIndex(l => new { l.TenantId, l.LeaveRequestId, l.CreatedAt });
        builder.HasIndex(l => new { l.TenantId, l.Status, l.NextRetryAt });
    }
}
