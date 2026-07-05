using System.Text.Json;

namespace HrPortal.Tenancy.Domain;

public sealed class Tenant
{
    private static readonly JsonSerializerOptions JsonOptions = new();
    private const TenantPlan DefaultPlan = TenantPlan.Enterprise;

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? Plan { get; private set; }

    /// <summary>JSON array of enabled module keys (e.g. "employees", "leave").</summary>
    public string? ModulesJson { get; private set; }

    /// <summary>JSON object of partial <see cref="TenantFeaturesOverrides"/> layered on plan defaults.</summary>
    public string? FeaturesJson { get; private set; }

    public DateTime? SuspendedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsSuspended => SuspendedAt.HasValue;

    private Tenant() { }

    public static Tenant Create(string name, string slug)
    {
        return new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug.ToLowerInvariant(),
            IsActive = true,
            Plan = DefaultPlan.ToString(),
            ModulesJson = "[]",
            FeaturesJson = null,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public TenantPlan GetPlan() =>
        !string.IsNullOrWhiteSpace(Plan) && Enum.TryParse<TenantPlan>(Plan, ignoreCase: true, out var plan)
            ? plan
            : DefaultPlan;

    public void SetPlan(TenantPlan plan) => Plan = plan.ToString();

    /// <summary>Module list (e.g. which business modules are enabled for this tenant).</summary>
    public IReadOnlyList<string> GetModules() =>
        string.IsNullOrWhiteSpace(ModulesJson)
            ? []
            : JsonSerializer.Deserialize<List<string>>(ModulesJson, JsonOptions) ?? [];

    public void SetModules(IReadOnlyList<string> modules) =>
        ModulesJson = JsonSerializer.Serialize(modules, JsonOptions);

    /// <summary>Partial per-tenant feature overrides layered on top of plan defaults.</summary>
    public TenantFeaturesOverrides GetFeatureOverrides() =>
        string.IsNullOrWhiteSpace(FeaturesJson)
            ? TenantFeaturesOverrides.Empty
            : JsonSerializer.Deserialize<TenantFeaturesOverrides>(FeaturesJson, JsonOptions) ?? TenantFeaturesOverrides.Empty;

    public void SetFeatureOverrides(TenantFeaturesOverrides overrides) =>
        FeaturesJson = JsonSerializer.Serialize(overrides, JsonOptions);

    /// <summary>Resolved feature set: plan defaults merged with tenant-specific overrides.</summary>
    public TenantFeatures GetEffectiveFeatures()
    {
        var defaults = TenantFeaturesDefaults.ForPlan(GetPlan());
        var overrides = GetFeatureOverrides();

        return new TenantFeatures(
            overrides.MaxEmployees ?? defaults.MaxEmployees,
            overrides.CustomRoles ?? defaults.CustomRoles,
            overrides.AuditLog ?? defaults.AuditLog,
            overrides.AdvancedReports ?? defaults.AdvancedReports);
    }

    public void Suspend() => SuspendedAt = DateTime.UtcNow;

    public void Unsuspend() => SuspendedAt = null;
}
