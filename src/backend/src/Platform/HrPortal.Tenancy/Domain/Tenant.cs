using System.Text.Json;

namespace HrPortal.Tenancy.Domain;

public sealed class Tenant
{
    private static readonly JsonSerializerOptions JsonOptions = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? Plan { get; private set; }
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
            Plan = "standard",
            FeaturesJson = "[]",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void SetPlan(string plan) => Plan = plan;

    public void SetFeatures(IReadOnlyList<string> features) =>
        FeaturesJson = JsonSerializer.Serialize(features, JsonOptions);

    public IReadOnlyList<string> GetFeatures() =>
        string.IsNullOrWhiteSpace(FeaturesJson)
            ? []
            : JsonSerializer.Deserialize<List<string>>(FeaturesJson, JsonOptions) ?? [];

    public void Suspend() => SuspendedAt = DateTime.UtcNow;

    public void Unsuspend() => SuspendedAt = null;
}
