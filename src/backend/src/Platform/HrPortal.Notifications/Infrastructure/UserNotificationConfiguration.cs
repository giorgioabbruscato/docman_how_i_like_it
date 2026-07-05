using HrPortal.Notifications.Application.Dtos;
using HrPortal.Notifications.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Notifications.Infrastructure;

internal sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("user_notifications", "notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type).HasMaxLength(100).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.MetadataJson).HasMaxLength(4000);

        builder.HasIndex(n => new { n.TenantId, n.RecipientUserId, n.CreatedAt });
        builder.HasIndex(n => new { n.TenantId, n.RecipientUserId, n.IsRead });
    }
}
