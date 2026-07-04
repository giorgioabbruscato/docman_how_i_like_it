using System.Net;
using System.Net.Http.Json;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class TenantIsolationTests : IntegrationTestBase
{
    public TenantIsolationTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetEmployeeById_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenantASlug = $"a{Guid.NewGuid():N}"[..10].ToLowerInvariant();
        var tenantBSlug = $"b{Guid.NewGuid():N}"[..10].ToLowerInvariant();

        using var setupClient = CreateClient(includeTenantHeader: false);
        var tenantAResponse = await setupClient.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Tenant A",
            slug = tenantASlug
        });
        tenantAResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var tenantBResponse = await setupClient.PostAsJsonAsync("/api/v1/tenants", new
        {
            name = "Tenant B",
            slug = tenantBSlug
        });
        tenantBResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        using var tenantAClient = CreateClientForTenant(tenantASlug, "hr");
        var email = $"isolated.{Guid.NewGuid():N}@example.com";
        var createResponse = await tenantAClient.PostAsJsonAsync("/api/v1/employees", new
        {
            firstName = "Isolated",
            lastName = "Employee",
            email,
            hireDate = "2024-01-15"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeIdResponse>();
        created.Should().NotBeNull();

        using var tenantBClient = CreateClientForTenant(tenantBSlug, "hr");
        var crossTenantResponse = await tenantBClient.GetAsync($"/api/v1/employees/{created!.Id}");

        crossTenantResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record EmployeeIdResponse(Guid Id);
}
