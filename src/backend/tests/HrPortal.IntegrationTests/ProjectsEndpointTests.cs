using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Audit.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using HrPortal.Projects.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class ProjectsEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProjectsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WithoutAuth()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantSlug);

        var response = await client.GetAsync("/api/v1/projects");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/projects");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.PostAsJsonAsync("/api/v1/projects", new
        {
            name = "Website Redesign",
            status = "Active",
            customerName = "Acme Corp"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProjectResponse>(JsonOptions);
        body!.Name.Should().Be("Website Redesign");
        body.CustomerName.Should().Be("Acme Corp");
        body.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync($"/api/v1/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrudHappyPath_WorksForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");

        var createResponse = await client.PostAsJsonAsync("/api/v1/projects", new
        {
            name = "Mobile App",
            status = "Active",
            description = "iOS and Android",
            customerName = "Beta Inc",
            startDate = "2025-01-01",
            endDate = "2025-12-31",
            budgetHours = 500,
            budgetCost = 50000
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>(JsonOptions);

        var getResponse = await client.GetAsync($"/api/v1/projects/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ProjectResponse>(JsonOptions);
        fetched!.Name.Should().Be("Mobile App");

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/projects/{created.Id}", new
        {
            name = "Mobile App v2",
            status = "OnHold",
            customerName = "Beta Inc"
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProjectResponse>(JsonOptions);
        updated!.Name.Should().Be("Mobile App v2");
        updated.Status.Should().Be("OnHold");

        var deleteResponse = await client.DeleteAsync($"/api/v1/projects/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var archivedGet = await client.GetAsync($"/api/v1/projects/{created.Id}");
        archivedGet.StatusCode.Should().Be(HttpStatusCode.OK);
        var archived = await archivedGet.Content.ReadFromJsonAsync<ProjectResponse>(JsonOptions);
        archived!.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task GetAll_SupportsPaginationAndFilters()
    {
        using var client = CreateAuthenticatedClient("hr");

        await client.PostAsJsonAsync("/api/v1/projects", new
        {
            name = "Alpha Searchable",
            status = "Active",
            customerName = "FilterCo"
        });
        await client.PostAsJsonAsync("/api/v1/projects", new
        {
            name = "Beta Other",
            status = "Completed",
            customerName = "OtherCo"
        });

        var searchResponse = await client.GetAsync("/api/v1/projects?search=searchable&page=1&pageSize=10");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchPage = await searchResponse.Content.ReadFromJsonAsync<PagedProjectsResponse>(JsonOptions);
        searchPage!.Items.Should().Contain(p => p.Name.Contains("Searchable", StringComparison.OrdinalIgnoreCase));
        searchPage.Items.Should().NotContain(p => p.Name == "Beta Other");

        var customerResponse = await client.GetAsync("/api/v1/projects?customerName=FilterCo");
        customerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var customerPage = await customerResponse.Content.ReadFromJsonAsync<PagedProjectsResponse>(JsonOptions);
        customerPage!.Items.Should().OnlyContain(p => p.CustomerName == "FilterCo");

        var statusResponse = await client.GetAsync("/api/v1/projects?status=Completed");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusPage = await statusResponse.Content.ReadFromJsonAsync<PagedProjectsResponse>(JsonOptions);
        statusPage!.Items.Should().OnlyContain(p => p.Status == "Completed");

        var archivedResponse = await client.GetAsync("/api/v1/projects?isArchived=false");
        archivedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var archivedPage = await archivedResponse.Content.ReadFromJsonAsync<PagedProjectsResponse>(JsonOptions);
        archivedPage!.Items.Should().OnlyContain(p => !p.IsArchived);
    }

    [Fact]
    public async Task Create_WritesAuditEntry()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.PostAsJsonAsync("/api/v1/projects", new
        {
            name = "Audit Project",
            status = "Active"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ProjectResponse>(JsonOptions);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var audit = await db.Set<AuditLog>()
            .IgnoreQueryFilters()
            .Where(a => a.EntityId == body!.Id.ToString() && a.Action == "project.created")
            .SingleOrDefaultAsync();

        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task Projects_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantAClient, "isolated");

        var listResponse = await tenantBClient.GetAsync("/api/v1/projects");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedProjectsResponse>(JsonOptions);
        page!.Items.Should().NotContain(p => p.Id == projectId);
    }

    [Fact]
    public async Task Projects_GetById_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantAClient, "get");

        var response = await tenantBClient.GetAsync($"/api/v1/projects/{projectId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Projects_Update_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantAClient, "update");

        var response = await tenantBClient.PutAsJsonAsync($"/api/v1/projects/{projectId}", new
        {
            name = "Hacked",
            status = "Active"
        });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Projects_Delete_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantAClient, "delete");

        var response = await tenantBClient.DeleteAsync($"/api/v1/projects/{projectId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<TenantIsolationFixture.TwoTenantSetup> CreateTwoTenantsAsync()
    {
        using var setupClient = CreateClient("admin", includeTenantHeader: false);
        return await TenantIsolationFixture.CreateTwoTenantsAsync(setupClient);
    }

    private sealed record ProjectResponse(
        Guid Id,
        string Name,
        string? CustomerName,
        string Status,
        bool IsArchived);

    private sealed record PagedProjectsResponse(
        List<ProjectResponse> Items,
        int TotalCount,
        int Page,
        int PageSize);
}
