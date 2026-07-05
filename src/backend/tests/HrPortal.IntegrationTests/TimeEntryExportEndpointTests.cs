using System.Net;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class TimeEntryExportEndpointTests : IntegrationTestBase
{
    public TimeEntryExportEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Export_ReturnsCsvContentType()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync("/api/v1/time-entries/export?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
    }

    [Fact]
    public async Task Export_ReturnsXlsxContentType()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync("/api/v1/time-entries/export?format=xlsx");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should()
            .Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    [Fact]
    public async Task Export_AppliesDateRangeFilter()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await client.GetAsync(
            "/api/v1/time-entries/export?format=csv&fromDate=2025-01-01&toDate=2025-12-31");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Export_ManagerCanAccessTeamScope()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var client = CreateAuthenticatedClient("guest", managerUserId);

        var response = await client.GetAsync("/api/v1/time-entries/export?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Export_ReturnsOk_ForEmployeeWithSelfReadPermission()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var userId = Guid.NewGuid();
        var roleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(hrClient, SystemRoleTemplates.EmployeeSlug);
        var employeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient);
        await TenantIsolationFixture.CreateMembershipAsync(hrClient, userId, roleId, employeeId);

        using var client = CreateAuthenticatedClient("guest", userId);

        var response = await client.GetAsync("/api/v1/time-entries/export?format=csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
