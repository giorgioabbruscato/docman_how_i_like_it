using System.Net;
using System.Net.Http.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

/// <summary>
/// Task 24 acceptance criteria: SaaS plan/feature gates and platform administrator endpoints.
/// </summary>
public sealed class PlatformAdminAndFeatureGateTests : IntegrationTestBase
{
    public PlatformAdminAndFeatureGateTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task PlatformRoutes_ReturnUnauthorized_WhenAnonymous()
    {
        using var client = CreateClient(includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/platform/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PlatformRoutes_ReturnForbidden_WhenAuthenticatedNonPlatformAdmin()
    {
        using var client = CreateClient("admin", includeTenantHeader: false);

        var response = await client.GetAsync("/api/v1/platform/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PlatformAdmin_CanListAndSuspendAndReactivateTenants()
    {
        var platformAdminId = await CreatePlatformAdminUserAsync();
        using var platformClient = CreateClient("platformadmin", platformAdminId, includeTenantHeader: false);

        var tenantSlug = await CreateTenantAsync();

        var listResponse = await platformClient.GetAsync("/api/v1/platform/tenants");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tenants = await listResponse.Content.ReadFromJsonAsync<List<PlatformTenantDto>>();
        var tenant = tenants!.Single(t => t.Slug == tenantSlug);
        tenant.IsSuspended.Should().BeFalse();

        var suspendResponse = await platformClient.PostAsync($"/api/v1/platform/tenants/{tenant.Id}/suspend", null);
        suspendResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var tenantClient = CreateClientForTenant(tenantSlug, "admin");
        var blockedResponse = await tenantClient.GetAsync("/api/v1/employees");
        blockedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var reactivateResponse = await platformClient.PostAsync($"/api/v1/platform/tenants/{tenant.Id}/reactivate", null);
        reactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterReactivate = await tenantClient.GetAsync("/api/v1/employees");
        afterReactivate.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PlatformAdmin_CanChangeTenantPlanAndFeatureOverrides()
    {
        var platformAdminId = await CreatePlatformAdminUserAsync();
        using var platformClient = CreateClient("platformadmin", platformAdminId, includeTenantHeader: false);

        var tenantSlug = await CreateTenantAsync();
        var tenantId = await GetTenantIdAsync(platformClient, tenantSlug);

        var planResponse = await platformClient.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/plan",
            new { plan = "Free" });
        planResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var planDto = await planResponse.Content.ReadFromJsonAsync<PlatformTenantDto>();
        planDto!.Plan.Should().Be("Free");
        planDto.Features.MaxEmployees.Should().Be(20);
        planDto.Features.CustomRoles.Should().BeFalse();

        var featuresResponse = await platformClient.PutAsJsonAsync(
            $"/api/v1/platform/tenants/{tenantId}/features",
            new { maxEmployees = 1 });
        featuresResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var featuresDto = await featuresResponse.Content.ReadFromJsonAsync<PlatformTenantDto>();
        featuresDto!.Features.MaxEmployees.Should().Be(1);
    }

    [Fact]
    public async Task EmployeeCreation_IsBlocked_WhenPlanLimitReached()
    {
        var platformAdminId = await CreatePlatformAdminUserAsync();
        using var platformClient = CreateClient("platformadmin", platformAdminId, includeTenantHeader: false);

        var tenantSlug = await CreateTenantAsync();
        var tenantId = await GetTenantIdAsync(platformClient, tenantSlug);

        await platformClient.PutAsJsonAsync($"/api/v1/platform/tenants/{tenantId}/plan", new { plan = "Free" });
        await platformClient.PutAsJsonAsync($"/api/v1/platform/tenants/{tenantId}/features", new { maxEmployees = 1 });

        using var tenantClient = CreateClientForTenant(tenantSlug, "admin");

        await TenantIsolationFixture.CreateEmployeeAsync(tenantClient);

        var response = await tenantClient.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Over",
            lastName = "Limit",
            email = $"over.{Guid.NewGuid():N}@demo.local",
            hireDate = "2024-01-15"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CustomRoleCreation_IsBlocked_OnFreePlan_AndAllowedAfterUpgrade()
    {
        var platformAdminId = await CreatePlatformAdminUserAsync();
        using var platformClient = CreateClient("platformadmin", platformAdminId, includeTenantHeader: false);

        var tenantSlug = await CreateTenantAsync();
        var tenantId = await GetTenantIdAsync(platformClient, tenantSlug);

        await platformClient.PutAsJsonAsync($"/api/v1/platform/tenants/{tenantId}/plan", new { plan = "Free" });

        using var tenantClient = CreateClientForTenant(tenantSlug, "admin");
        var slug = $"custom-{Guid.NewGuid():N}"[..16];

        var blockedResponse = await tenantClient.PostAsJsonAsync("/api/v1/roles", new
        {
            slug,
            permissions = new[] { "employee.read:self" }
        });
        blockedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await platformClient.PutAsJsonAsync($"/api/v1/platform/tenants/{tenantId}/features", new { customRoles = true });

        var allowedResponse = await tenantClient.PostAsJsonAsync("/api/v1/roles", new
        {
            slug,
            permissions = new[] { "employee.read:self" }
        });
        allowedResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private async Task<Guid> CreatePlatformAdminUserAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();

        var userId = Guid.NewGuid();
        dbContext.Set<UserProfile>().Add(
            UserProfile.Create(userId, $"pa.{userId:N}@demo.local", isPlatformAdmin: true));
        await dbContext.SaveChangesAsync();

        return userId;
    }

    private async Task<string> CreateTenantAsync()
    {
        var slug = $"fg{Guid.NewGuid():N}"[..10].ToLowerInvariant();
        var response = await Client.PostAsJsonAsync("/api/v1/tenants", new { name = "Feature Gate Tenant", slug });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return slug;
    }

    private static async Task<Guid> GetTenantIdAsync(HttpClient platformClient, string slug)
    {
        var response = await platformClient.GetAsync("/api/v1/platform/tenants");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tenants = await response.Content.ReadFromJsonAsync<List<PlatformTenantDto>>();
        return tenants!.Single(t => t.Slug == slug).Id;
    }

    private sealed record PlatformTenantDto(
        Guid Id,
        string Name,
        string Slug,
        bool IsActive,
        bool IsSuspended,
        string Plan,
        List<string> Modules,
        FeaturesDto Features);

    private sealed record FeaturesDto(
        int MaxEmployees,
        bool CustomRoles,
        bool AuditLog,
        bool AdvancedReports);
}
