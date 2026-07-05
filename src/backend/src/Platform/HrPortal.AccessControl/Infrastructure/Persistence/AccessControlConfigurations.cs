using HrPortal.AccessControl.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.AccessControl.Infrastructure.Persistence;

internal sealed class TenantRoleConfiguration : IEntityTypeConfiguration<TenantRole>
{
    public void Configure(EntityTypeBuilder<TenantRole> builder)
    {
        builder.ToTable("tenant_roles", "platform");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => new { r.TenantId, r.Slug })
            .IsUnique();

        builder.Property(r => r.PermissionsJson)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(r => r.IsSystem)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired();
    }
}

internal sealed class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>
{
    public void Configure(EntityTypeBuilder<TenantMembership> builder)
    {
        builder.ToTable("tenant_memberships", "platform");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.HasIndex(m => new { m.TenantId, m.UserId })
            .IsUnique();

        builder.Property(m => m.RoleIdsJson)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(m => m.AttributesJson)
            .HasMaxLength(4000);

        builder.Property(m => m.IsActive)
            .IsRequired();
    }
}

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles", "platform");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .IsUnique();

        builder.HasIndex(p => p.Email)
            .IsUnique();

        builder.Property(p => p.IsPlatformAdmin)
            .IsRequired();
    }
}
