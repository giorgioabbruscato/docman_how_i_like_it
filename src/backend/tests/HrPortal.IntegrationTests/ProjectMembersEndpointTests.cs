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

public sealed class ProjectMembersEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProjectMembersEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AddMember_ReturnsCreated_ForHrRole()
    {
        using var client = CreateAuthenticatedClient("hr");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "member");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "team");

        var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Lead",
            hourlyRate = 85.00
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MemberResponse>(JsonOptions);
        body!.EmployeeId.Should().Be(employeeId);
        body.Role.Should().Be("Lead");
        body.HourlyRate.Should().Be(85.00m);
    }

    [Fact]
    public async Task AddMember_ReturnsConflict_WhenDuplicate()
    {
        using var client = CreateAuthenticatedClient("hr");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "dup");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "dup");

        var first = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Member"
        });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Observer"
        });
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddMember_ReturnsNotFound_WhenEmployeeInactive()
    {
        using var client = CreateAuthenticatedClient("hr");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "inactive");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "inactive");

        var deactivate = await client.DeleteAsync($"/api/v1/employees/{employeeId}");
        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Member"
        });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMembers_ReturnsList()
    {
        using var client = CreateAuthenticatedClient("hr");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "list");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "list");

        await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Member"
        });

        var response = await client.GetAsync($"/api/v1/projects/{projectId}/members");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await response.Content.ReadFromJsonAsync<List<MemberResponse>>(JsonOptions);
        members.Should().HaveCount(1);
        members![0].EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task RemoveMember_ReturnsNoContent()
    {
        using var client = CreateAuthenticatedClient("hr");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "remove");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "remove");

        var addResponse = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Member"
        });
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var member = await addResponse.Content.ReadFromJsonAsync<MemberResponse>(JsonOptions);

        var deleteResponse = await client.DeleteAsync($"/api/v1/projects/{projectId}/members/{member!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync($"/api/v1/projects/{projectId}/members");
        var members = await listResponse.Content.ReadFromJsonAsync<List<MemberResponse>>(JsonOptions);
        members.Should().BeEmpty();
    }

    [Fact]
    public async Task AddMember_WritesAuditEntry()
    {
        using var client = CreateAuthenticatedClient("hr");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(client, "audit");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(client, "audit");

        var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Member"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<MemberResponse>(JsonOptions);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HrPortalDbContext>();
        var audit = await db.Set<AuditLog>()
            .IgnoreQueryFilters()
            .Where(a => a.EntityId == body!.Id.ToString() && a.Action == "project.member.added")
            .SingleOrDefaultAsync();

        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task Members_List_DoesNotIncludeAnotherTenantsData()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(tenantAClient, "cross");
        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantAClient, "cross");
        await tenantAClient.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId,
            role = "Member"
        });

        var listResponse = await tenantBClient.GetAsync($"/api/v1/projects/{projectId}/members");
        listResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Members_Add_ReturnsNotFound_WhenAccessingAnotherTenantsProject()
    {
        var tenants = await CreateTwoTenantsAsync();
        using var tenantAClient = CreateClientForTenant(tenants.TenantASlug, "hr");
        using var tenantBClient = CreateClientForTenant(tenants.TenantBSlug, "hr");

        var projectId = await TenantIsolationFixture.CreateProjectAsync(tenantAClient, "add-cross");
        var employeeB = await TenantIsolationFixture.CreateEmployeeAsync(tenantBClient, "add-cross");

        var response = await tenantBClient.PostAsJsonAsync($"/api/v1/projects/{projectId}/members", new
        {
            employeeId = employeeB,
            role = "Member"
        });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<TenantIsolationFixture.TwoTenantSetup> CreateTwoTenantsAsync()
    {
        using var setupClient = CreateClient("admin", includeTenantHeader: false);
        return await TenantIsolationFixture.CreateTwoTenantsAsync(setupClient);
    }

    private sealed record MemberResponse(
        Guid Id,
        Guid ProjectId,
        Guid EmployeeId,
        string Role,
        decimal? HourlyRate);
}
