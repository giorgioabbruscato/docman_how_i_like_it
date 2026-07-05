using HrPortal.Workflows.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrPortal.Workflows.Infrastructure.Persistence;

internal sealed class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("workflow_definitions", "workflows");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.RequestType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.StepsJson).HasMaxLength(8000).IsRequired();

        builder.HasIndex(d => new { d.TenantId, d.RequestType, d.IsActive });
        builder.HasIndex(d => new { d.TenantId, d.RequestType, d.Version });
    }
}

internal sealed class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("workflow_instances", "workflows");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.RequestType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.HasIndex(i => new { i.TenantId, i.RequestType, i.RequestId });
        builder.HasIndex(i => new { i.TenantId, i.Status, i.CurrentStepIndex });
    }
}

internal sealed class WorkflowActionConfiguration : IEntityTypeConfiguration<WorkflowAction>
{
    public void Configure(EntityTypeBuilder<WorkflowAction> builder)
    {
        builder.ToTable("workflow_actions", "workflows");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(a => a.Comment).HasMaxLength(2000);

        builder.HasIndex(a => new { a.TenantId, a.WorkflowInstanceId, a.ActionAt });
    }
}
