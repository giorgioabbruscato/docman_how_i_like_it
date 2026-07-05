using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class TimeEntriesEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TimeEntriesEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsOk_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync("/api/v1/time-entries");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CrudHappyPath_WorksForEmployee()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var projectId = await CreateProjectAsHrAsync();
        var start = DateTime.UtcNow.AddHours(-3);
        var end = DateTime.UtcNow.AddHours(-1);

        var createResponse = await client.PostAsJsonAsync("/api/v1/time-entries", new
        {
            projectId,
            startTime = start,
            endTime = end,
            description = "Feature work",
            billable = true
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TimeEntryResponse>(JsonOptions);

        var getResponse = await client.GetAsync($"/api/v1/time-entries/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/time-entries/{created.Id}", new
        {
            projectId,
            startTime = start,
            endTime = end.AddMinutes(30),
            description = "Updated work",
            billable = false
        });
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TimeEntryResponse>(JsonOptions);
        updated!.Billable.Should().BeFalse();

        var deleteResponse = await client.DeleteAsync($"/api/v1/time-entries/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetAll_FiltersToSelf_ForEmployeeRole()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var otherEmployeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId);
        var projectId = await TenantIsolationFixture.CreateProjectAsync(hrClient, "scope");

        var otherUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(hrClient, otherUserId, employeeRoleId, otherEmployeeId);

        using var otherClient = CreateAuthenticatedClient("guest", otherUserId);
        await otherClient.PostAsJsonAsync("/api/v1/time-entries", new
        {
            projectId,
            startTime = DateTime.UtcNow.AddHours(-2),
            endTime = DateTime.UtcNow.AddHours(-1)
        });

        using var employeeClient = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var listResponse = await employeeClient.GetAsync("/api/v1/time-entries");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedTimeEntriesResponse>(JsonOptions);
        page!.Items.Should().NotContain(e => e.EmployeeId == otherEmployeeId);
    }

    [Fact]
    public async Task TimeEntries_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient);
        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantAClient);
        var entryId = await CreateTimeEntryForEmployeeAsync(tenantAClient, employeeId, projectId);

        var listResponse = await tenantBClient.GetAsync("/api/v1/time-entries");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedTimeEntriesResponse>(JsonOptions);
        page!.Items.Should().NotContain(e => e.Id == entryId);
    }

    private async Task<Guid> CreateProjectAsHrAsync()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        return await TenantIsolationFixture.CreateProjectAsync(hrClient);
    }

    private static async Task<Guid> CreateTimeEntryForEmployeeAsync(
        HttpClient adminClient,
        Guid employeeId,
        Guid projectId)
    {
        var userId = Guid.NewGuid();
        var roleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(adminClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(adminClient, userId, roleId, employeeId);

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/time-entries")
        {
            Content = System.Net.Http.Json.JsonContent.Create(new
            {
                projectId,
                startTime = DateTime.UtcNow.AddHours(-2),
                endTime = DateTime.UtcNow.AddHours(-1)
            })
        };
        request.Headers.Add("X-Tenant-Id", adminClient.DefaultRequestHeaders.GetValues("X-Tenant-Id").First());
        request.Headers.Add(TestAuthHandler.RoleHeaderName, "guest");
        request.Headers.Add(TestAuthHandler.UserIdHeaderName, userId.ToString());

        var response = await adminClient.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<TimeEntryResponse>(JsonOptions);
        return body!.Id;
    }

    private async Task<TenantIsolationFixture.TwoTenantSetup> CreateTwoTenantsAsync()
    {
        using var setupClient = CreateClient("admin", includeTenantHeader: false);
        return await TenantIsolationFixture.CreateTwoTenantsAsync(setupClient);
    }

    private sealed record TimeEntryResponse(
        Guid Id,
        Guid EmployeeId,
        Guid ProjectId,
        int WorkedMinutes,
        bool Billable);

    private sealed record PagedTimeEntriesResponse(
        List<TimeEntryResponse> Items,
        int TotalCount,
        int Page,
        int PageSize);
}
