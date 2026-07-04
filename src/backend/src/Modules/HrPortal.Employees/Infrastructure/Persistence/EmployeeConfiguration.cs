using HrPortal.Employees.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Employees.Infrastructure.Persistence;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees", "employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique();

        builder.Property(e => e.JobTitle)
            .HasMaxLength(150);

        builder.Property(e => e.IsActive)
            .IsRequired();
    }
}
