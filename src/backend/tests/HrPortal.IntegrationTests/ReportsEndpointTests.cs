using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class ReportsEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ReportsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Generate_ReturnsCsvContentType()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync("/api/v1/reports/employees?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    }

    [Fact]
    public async Task Generate_ReturnsXlsxContentType()
    {
        using var client = CreateAuthenticatedClient("hr");

        var response = await client.GetAsync("/api/v1/reports/departments?format=xlsx");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public async Task Generate_ReturnsForbidden_WithoutPermission()
    {
        using var adminClient = CreateAuthenticatedClient("admin");
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(adminClient, "no-report-perm");
        var userId = Guid.NewGuid();

        var roleResponse = await adminClient.PostAsJsonAsync("/api/v1/roles", new
        {
            slug = $"view-only-{Guid.NewGuid():N}"[..20],
            permissions = new[] { Permissions.DocumentReadSelf }
        });
        roleResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var role = await roleResponse.Content.ReadFromJsonAsync<RoleResponse>(JsonOptions);

        await TenantIsolationFixture.CreateMembershipAsync(adminClient, userId, role!.Id, employeeId);

        using var client = CreateAuthenticatedClient("guest", userId);

        var response = await client.GetAsync("/api/v1/reports/employees?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Generate_EmployeeSelfScope_ReturnsOk()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync("/api/v1/reports/employees?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record RoleResponse(Guid Id, string Slug);
}
