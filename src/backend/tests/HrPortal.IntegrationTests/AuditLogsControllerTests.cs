using System.Net;
using System.Net.Http.Json;
using HrPortal.Audit.Application;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

/// <summary>
/// Task 25 acceptance criteria: enterprise audit log query API, permission-gated and feature-gated.
/// </summary>
public sealed class AuditLogsControllerTests : IntegrationTestBase
{
    public AuditLogsControllerTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAuditLogs_ReturnsUnauthorized_WhenAnonymous()
    {
        using var client = CreateClient(includeTenantHeader: true);

        var response = await client.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsForbidden_WhenMissingPermission()
    {
        using var client = CreateClient("employee");

        var response = await client.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsOk_ForAdminOnEnterpriseTenant()
    {
        // Seed at least one access decision so the demo tenant has audit rows to page through.
        using var seedClient = CreateClient("admin");
        await seedClient.GetAsync("/api/v1/employees");

        using var client = CreateClient("admin");

        var response = await client.GetAsync("/api/v1/audit-logs?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AuditLogDto>>();
        page.Should().NotBeNull();
        page!.Items.Should().NotBeEmpty();
        page.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsForbidden_WhenFeatureDisabledOnFreePlan()
    {
        var tenantSlug = await CreateTenantAsync();
        var platformAdminId = await CreatePlatformAdminUserAsync();
        using var platformClient = CreateClient("platformadmin", platformAdminId, includeTenantHeader: false);
        var tenantId = await GetTenantIdAsync(platformClient, tenantSlug);

        await platformClient.PutAsJsonAsync($"/api/v1/platform/tenants/{tenantId}/plan", new { plan = "Free" });

        using var tenantClient = CreateClientForTenant(tenantSlug, "admin");

        var response = await tenantClient.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAuditLogs_ReturnsOk_AfterFeatureOverrideEnabled()
    {
        var tenantSlug = await CreateTenantAsync();
        var platformAdminId = await CreatePlatformAdminUserAsync();
        using var platformClient = CreateClient("platformadmin", platformAdminId, includeTenantHeader: false);
        var tenantId = await GetTenantIdAsync(platformClient, tenantSlug);

        await platformClient.PutAsJsonAsync($"/api/v1/platform/tenants/{tenantId}/plan", new { plan = "Free" });
        await platformClient.PutAsJsonAsync($"/api/v1/platform/tenants/{tenantId}/features", new { auditLog = true });

        using var tenantClient = CreateClientForTenant(tenantSlug, "admin");
        await tenantClient.GetAsync("/api/v1/employees");

        var response = await tenantClient.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task<Guid> CreatePlatformAdminUserAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HrPortal.Api.Infrastructure.Persistence.HrPortalDbContext>();

        var userId = Guid.NewGuid();
        dbContext.Set<HrPortal.AccessControl.Domain.UserProfile>().Add(
            HrPortal.AccessControl.Domain.UserProfile.Create(userId, $"pa.{userId:N}@demo.local", isPlatformAdmin: true));
        await dbContext.SaveChangesAsync();

        return userId;
    }

    private async Task<string> CreateTenantAsync()
    {
        var slug = $"al{Guid.NewGuid():N}"[..10].ToLowerInvariant();
        var response = await Client.PostAsJsonAsync("/api/v1/tenants", new { name = "Audit Log Tenant", slug });
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

    private sealed record PlatformTenantDto(Guid Id, string Name, string Slug, bool IsActive, bool IsSuspended, string Plan);
}
