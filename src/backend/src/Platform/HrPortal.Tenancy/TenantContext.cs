namespace HrPortal.Tenancy;

public sealed class TenantContext
{
    public Guid TenantId { get; init; }
    public string TenantSlug { get; init; } = string.Empty;
    public bool IsResolved { get; init; }

    public static TenantContext Empty => new() { IsResolved = false };

    public static TenantContext Create(Guid tenantId, string slug) =>
        new() { TenantId = tenantId, TenantSlug = slug, IsResolved = true };
}
