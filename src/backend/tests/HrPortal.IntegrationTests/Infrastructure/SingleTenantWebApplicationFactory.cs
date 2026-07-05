namespace HrPortal.IntegrationTests.Infrastructure;

public sealed class SingleTenantWebApplicationFactory : HrPortalWebApplicationFactory
{
    protected override IReadOnlyDictionary<string, string?>? ConfigOverrides { get; } =
        new Dictionary<string, string?>
        {
            ["Tenancy:Mode"] = "Single",
            ["Tenancy:DefaultTenantSlug"] = "demo"
        };
}
