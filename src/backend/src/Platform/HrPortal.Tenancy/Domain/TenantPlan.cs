namespace HrPortal.Tenancy.Domain;

/// <summary>SaaS subscription tier. Single-tenant OSS deployments are treated as Enterprise-equivalent.</summary>
public enum TenantPlan
{
    Free = 0,
    Pro = 1,
    Enterprise = 2
}
