using HrPortal.Calendar.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Calendar.Infrastructure.Persistence;

internal sealed class PublicHolidayConfiguration : IEntityTypeConfiguration<PublicHoliday>
{
    public void Configure(EntityTypeBuilder<PublicHoliday> builder)
    {
        builder.ToTable("public_holidays", "calendar");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).HasMaxLength(200).IsRequired();
        builder.Property(h => h.CountryCode).HasMaxLength(10);
        builder.HasIndex(h => new { h.TenantId, h.Date });
    }
}

internal sealed class SmartWorkingScheduleConfiguration : IEntityTypeConfiguration<SmartWorkingSchedule>
{
    public void Configure(EntityTypeBuilder<SmartWorkingSchedule> builder)
    {
        builder.ToTable("smart_working_schedules", "calendar");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.WeekdaysJson).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.TenantId).IsUnique();
    }
}
