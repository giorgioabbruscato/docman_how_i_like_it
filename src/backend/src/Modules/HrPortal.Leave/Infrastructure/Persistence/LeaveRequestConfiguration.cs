using HrPortal.Leave.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Leave.Infrastructure.Persistence;

internal sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_requests", "leave");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.Reason)
            .HasMaxLength(500);

        builder.HasIndex(l => new { l.TenantId, l.EmployeeId, l.StartDate, l.EndDate });
    }
}
