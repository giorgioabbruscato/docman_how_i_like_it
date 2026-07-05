using HrPortal.Tenancy.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Tenancy.Infrastructure.Persistence;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", "platform");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.Slug)
            .IsUnique();

        builder.Property(t => t.IsActive)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.Plan)
            .HasMaxLength(50);

        builder.Property(t => t.ModulesJson)
            .HasMaxLength(2000);

        builder.Property(t => t.FeaturesJson)
            .HasMaxLength(2000);

        builder.Property(t => t.SuspendedAt);
    }
}
