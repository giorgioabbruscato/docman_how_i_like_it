using FluentAssertions;
using HrPortal.Tenancy;
using HrPortal.Tenancy.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HrPortal.UnitTests.Tenancy;

public sealed class TenantResolverTests
{
    private static TenantResolver CreateResolver(TenantResolverOptions options) =>
        new(Options.Create(options));

    private static DefaultHttpContext CreateHttpContext(
        string? tenantHeader = null,
        string host = "localhost")
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);

        if (tenantHeader is not null)
            context.Request.Headers["X-Tenant-Id"] = tenantHeader;

        return context;
    }

    [Fact]
    public async Task ResolveSlugAsync_MultiMode_NoHeader_ReturnsNull()
    {
        var resolver = CreateResolver(new TenantResolverOptions { Mode = TenantDeploymentMode.Multi });
        var context = CreateHttpContext();

        var slug = await resolver.ResolveSlugAsync(context);

        slug.Should().BeNull();
    }

    [Fact]
    public async Task ResolveSlugAsync_SingleMode_NoHeader_ReturnsDefaultTenantSlug()
    {
        var resolver = CreateResolver(new TenantResolverOptions
        {
            Mode = TenantDeploymentMode.Single,
            DefaultTenantSlug = "demo"
        });
        var context = CreateHttpContext();

        var slug = await resolver.ResolveSlugAsync(context);

        slug.Should().Be("demo");
    }

    [Fact]
    public async Task ResolveSlugAsync_SingleMode_HeaderPresent_ReturnsHeaderValue()
    {
        var resolver = CreateResolver(new TenantResolverOptions
        {
            Mode = TenantDeploymentMode.Single,
            DefaultTenantSlug = "demo"
        });
        var context = CreateHttpContext(tenantHeader: "acme");

        var slug = await resolver.ResolveSlugAsync(context);

        slug.Should().Be("acme");
    }

    [Fact]
    public async Task ResolveSlugAsync_SubdomainResolution_PreferredOverSingleModeDefault()
    {
        var resolver = CreateResolver(new TenantResolverOptions
        {
            Mode = TenantDeploymentMode.Single,
            DefaultTenantSlug = "demo",
            UseSubdomainResolution = true,
            BaseDomain = "hrportal.local"
        });
        var context = CreateHttpContext(host: "acme.hrportal.local");

        var slug = await resolver.ResolveSlugAsync(context);

        slug.Should().Be("acme");
    }
}
