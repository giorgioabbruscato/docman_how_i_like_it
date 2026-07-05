using HrPortal.Attendance.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Attendance.Infrastructure.Persistence;

internal sealed class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
{
    public void Configure(EntityTypeBuilder<AttendanceSession> builder)
    {
        builder.ToTable("attendance_sessions", "attendance");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.IPAddress)
            .HasMaxLength(45);

        builder.Property(s => s.Device)
            .HasMaxLength(200);

        builder.Property(s => s.Browser)
            .HasMaxLength(200);

        builder.HasIndex(s => new { s.TenantId, s.EmployeeId, s.Status })
            .HasFilter("\"Status\" = 'Open'")
            .IsUnique();
    }
}

internal sealed class GeofenceZoneConfiguration : IEntityTypeConfiguration<GeofenceZone>
{
    public void Configure(EntityTypeBuilder<GeofenceZone> builder)
    {
        builder.ToTable("geofence_zones", "attendance");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Name).HasMaxLength(200).IsRequired();
        builder.Property(z => z.Description).HasMaxLength(1000);
        builder.HasIndex(z => new { z.TenantId, z.IsActive });
    }
}

internal sealed class GeofenceSettingsConfiguration : IEntityTypeConfiguration<GeofenceSettings>
{
    public void Configure(EntityTypeBuilder<GeofenceSettings> builder)
    {
        builder.ToTable("geofence_settings", "attendance");
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.TenantId).IsUnique();
    }
}
