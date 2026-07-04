namespace HrPortal.SharedKernel.Entities;

public abstract class AuditableEntity : Entity, ITenantEntity
{
    public Guid TenantId { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    protected void SetTenant(Guid tenantId) => TenantId = tenantId;

    public void MarkUpdated(Guid? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }
}
