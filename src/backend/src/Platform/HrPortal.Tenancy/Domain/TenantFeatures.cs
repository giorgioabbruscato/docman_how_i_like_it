namespace HrPortal.Tenancy.Domain;

/// <summary>Effective (resolved) feature set for a tenant — plan defaults merged with overrides.</summary>
public sealed record TenantFeatures(
    int MaxEmployees,
    bool CustomRoles,
    bool AuditLog,
    bool AdvancedReports);

/// <summary>Partial per-tenant feature overrides layered on top of plan defaults. Null = inherit plan default.</summary>
public sealed record TenantFeaturesOverrides(
    int? MaxEmployees = null,
    bool? CustomRoles = null,
    bool? AuditLog = null,
    bool? AdvancedReports = null)
{
    public static readonly TenantFeaturesOverrides Empty = new();
}

/// <summary>Canonical feature flag keys used by <c>IFeatureGateService</c>.</summary>
public static class FeatureKeys
{
    public const string CustomRoles = "customRoles";
    public const string AuditLog = "auditLog";
    public const string AdvancedReports = "advancedReports";
}

public static class TenantFeaturesDefaults
{
    public static TenantFeatures ForPlan(TenantPlan plan) => plan switch
    {
        TenantPlan.Free => new TenantFeatures(MaxEmployees: 20, CustomRoles: false, AuditLog: false, AdvancedReports: false),
        TenantPlan.Pro => new TenantFeatures(MaxEmployees: 200, CustomRoles: true, AuditLog: true, AdvancedReports: false),
        TenantPlan.Enterprise => new TenantFeatures(MaxEmployees: int.MaxValue, CustomRoles: true, AuditLog: true, AdvancedReports: true),
        _ => new TenantFeatures(MaxEmployees: 20, CustomRoles: false, AuditLog: false, AdvancedReports: false)
    };
}
