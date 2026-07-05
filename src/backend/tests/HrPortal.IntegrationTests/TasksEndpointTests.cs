using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.Audit.Domain;
using HrPortal.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HrPortal.IntegrationTests;

public sealed class TasksEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TasksEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsUnauthorized_WithoutAuth()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantSlug);

        var response = await client.GetAsync("/api/v1/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "task-proj");

        var response = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Implement login",
            priority = "High",
            status = "Todo"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        body!.Title.Should().Be("Implement login");
        body.Priority.Should().Be("High");
        body.Status.Should().Be("Todo");
        body.ProjectId.Should().Be(projectId);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenProjectMissing()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId = Guid.NewGuid(),
            title = "Orphan task",
            priority = "Medium"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenEmployeeInactive()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "inactive-emp");

        var response = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Assigned task",
            priority = "Medium",
            assignedEmployeeId = Guid.NewGuid()
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync($"/api/v1/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrudHappyPath_WorksForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "crud");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "assignee");

        var createResponse = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Design API",
            priority = "High",
            status = "Todo",
            description = "REST endpoints",
            assignedEmployeeId = employeeId,
            estimatedHours = 8,
            dueDate = "2025-12-31"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        var getResponse = await client.GetAsync($"/api/v1/tasks/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        fetched!.Title.Should().Be("Design API");
        fetched.AssignedEmployeeId.Should().Be(employeeId);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/tasks/{created.Id}", new
        {
            projectId,
            title = "Design API v2",
            priority = "Critical",
            status = "InProgress",
            assignedEmployeeId = employeeId,
            spentHours = 2
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        updated!.Title.Should().Be("Design API v2");
        updated.Status.Should().Be("InProgress");
        updated.SpentHours.Should().Be(2);

        var deleteResponse = await client.DeleteAsync($"/api/v1/tasks/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deletedGet = await client.GetAsync($"/api/v1/tasks/{created.Id}");
        deletedGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_SupportsPaginationAndFilters()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectA = await TenantIsolationFixture.CreateProjectAsync(client, "filter-a");
        var projectB = await TenantIsolationFixture.CreateProjectAsync(client, "filter-b");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "filter-emp");

        await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId = projectA,
            title = "Alpha Searchable",
            priority = "High",
            status = "Todo",
            assignedEmployeeId = employeeId
        });
        await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId = projectB,
            title = "Beta Other",
            priority = "Low",
            status = "Done"
        });

        var searchResponse = await client.GetAsync("/api/v1/tasks?search=searchable&page=1&pageSize=10");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchPage = await searchResponse.Content.ReadFromJsonAsync<PagedTasksResponse>(JsonOptions);
        searchPage!.Items.Should().Contain(t => t.Title.Contains("Searchable", StringComparison.OrdinalIgnoreCase));
        searchPage.Items.Should().NotContain(t => t.Title == "Beta Other");

        var projectResponse = await client.GetAsync($"/api/v1/tasks?projectId={projectA}");
        projectResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectPage = await projectResponse.Content.ReadFromJsonAsync<PagedTasksResponse>(JsonOptions);
        projectPage!.Items.Should().OnlyContain(t => t.ProjectId == projectA);

        var statusResponse = await client.GetAsync("/api/v1/tasks?status=Done");
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusPage = await statusResponse.Content.ReadFromJsonAsync<PagedTasksResponse>(JsonOptions);
        statusPage!.Items.Should().OnlyContain(t => t.Status == "Done");

        var priorityResponse = await client.GetAsync("/api/v1/tasks?priority=High");
        priorityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var priorityPage = await priorityResponse.Content.ReadFromJsonAsync<PagedTasksResponse>(JsonOptions);
        priorityPage!.Items.Should().OnlyContain(t => t.Priority == "High");

        var assigneeResponse = await client.GetAsync($"/api/v1/tasks?assignedEmployeeId={employeeId}");
        assigneeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var assigneePage = await assigneeResponse.Content.ReadFromJsonAsync<PagedTasksResponse>(JsonOptions);
        assigneePage!.Items.Should().OnlyContain(t => t.AssignedEmployeeId == employeeId);
    }

    [Fact]
    public async Task Create_WritesAuditEntry()
    {
        using var client = CreateAuthenticatedClient("hr");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "audit");

        var response = await client.PostAsJsonAsync("/api/v1/tasks", new
        {
            projectId,
            title = "Audit Task",
            priority = "Medium"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var audit = await db.Set<AuditLog>()
            .IgnoreQueryFilters()
            .Where(a => a.EntityId == body!.Id.ToString() && a.Action == "task.created")
            .SingleOrDefaultAsync();

        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task Tasks_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var taskId = await TenantIsolationFixture.CreateProjectTaskAsync(tenantAClient, "isolated");

        var listResponse = await tenantBClient.GetAsync("/api/v1/tasks");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedTasksResponse>(JsonOptions);
        page!.Items.Should().NotContain(t => t.Id == taskId);
    }

    [Fact]
    public async Task Tasks_GetById_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var taskId = await TenantIsolationFixture.CreateProjectTaskAsync(tenantAClient, "get");

        var response = await tenantBClient.GetAsync($"/api/v1/tasks/{taskId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tasks_Update_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var taskId = await TenantIsolationFixture.CreateProjectTaskAsync(tenantAClient, "update");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantBClient, "other");

        var response = await tenantBClient.PutAsJsonAsync($"/api/v1/tasks/{taskId}", new
        {
            projectId,
            title = "Hacked",
            priority = "Low",
            status = "Todo"
        });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tasks_Delete_ReturnsNotFound_WhenAccessingAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var taskId = await TenantIsolationFixture.CreateProjectTaskAsync(tenantAClient, "delete");

        var response = await tenantBClient.DeleteAsync($"/api/v1/tasks/{taskId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<TenantIsolationFixture.TwoTenantSetup> CreateTwoTenantsAsync()
    {
        using var setupClient = CreateClient("admin", includeTenantHeader: false);
        return await TenantIsolationFixture.CreateTwoTenantsAsync(setupClient);
    }

    private sealed record TaskResponse(
        Guid Id,
        Guid ProjectId,
        string Title,
        Guid? AssignedEmployeeId,
        string Priority,
        string Status,
        decimal SpentHours);

    private sealed record PagedTasksResponse(
        List<TaskResponse> Items,
        int TotalCount,
        int Page,
        int PageSize);
}
