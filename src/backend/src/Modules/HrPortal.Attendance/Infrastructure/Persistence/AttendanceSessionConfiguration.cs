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
