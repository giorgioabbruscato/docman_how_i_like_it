using HrPortal.Audit.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Audit.Infrastructure.Persistence;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs", "platform");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Entity)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.EntityId)
            .HasMaxLength(100);

        builder.Property(a => a.Metadata)
            .HasColumnType("jsonb");

        builder.Property(a => a.TargetId)
            .HasMaxLength(100);

        builder.Property(a => a.Scope)
            .HasMaxLength(50);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(64);

        builder.Property(a => a.ActorEmail)
            .HasMaxLength(255);

        builder.Property(a => a.Decision)
            .HasMaxLength(10);

        builder.HasIndex(a => new { a.TenantId, a.Timestamp });
        builder.HasIndex(a => new { a.TenantId, a.Decision });
        builder.HasIndex(a => new { a.TenantId, a.Action });
    }
}
