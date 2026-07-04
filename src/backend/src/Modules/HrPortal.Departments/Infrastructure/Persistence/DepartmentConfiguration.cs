using HrPortal.Departments.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Departments.Infrastructure.Persistence;

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments", "departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(d => d.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(d => new { d.TenantId, d.Code })
            .IsUnique();

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.IsActive)
            .IsRequired();

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
