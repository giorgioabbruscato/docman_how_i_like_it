using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HrPortal.AccessControl.Application.Dtos;
using HrPortal.AccessControl.Domain;
using HrPortal.AccessControl.Infrastructure.Seeding;
using HrPortal.IntegrationTests.Infrastructure;

namespace HrPortal.IntegrationTests;

public sealed class LeaveRequestsEndpointTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public LeaveRequestsEndpointTests(HrPortalWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.GetAsync("/api/v1/leave-requests");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenEmployeeMissing()
    {
        using var client = CreateAuthenticatedClient("employee");

        var response = await client.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId = Guid.NewGuid(),
            startDate = "2025-07-01",
            endDate = "2025-07-05",
            type = "Annual"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ReturnsCreated_ForOwnEmployee()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var employeeId = await GetEmployeeIdAsync(client);

        var response = await client.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2025-08-01",
            endDate = "2025-08-05",
            type = "Annual"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ReturnsForbidden_ForOtherEmployee()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var otherEmployeeId = await TenantIsolationFixture.CreateEmployeeAsync(hrClient);

        using var employeeClient = CreateAuthenticatedClient("employee", DemoUsers.Employee);

        var response = await employeeClient.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId = otherEmployeeId,
            startDate = "2025-09-01",
            endDate = "2025-09-05",
            type = "Annual"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Cancel_ReturnsNoContent_ForOwnPendingLeave()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var employeeId = await GetEmployeeIdAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2025-10-01",
            endDate = "2025-10-03",
            type = "Sick"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var cancelResponse = await client.DeleteAsync($"/api/v1/leave-requests/{leave!.Id}");
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Approve_ReturnsForbidden_ForEmployeeRole()
    {
        using var client = CreateAuthenticatedClient("employee", DemoUsers.Employee);
        var employeeId = await GetEmployeeIdAsync(client);

        var createResponse = await client.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2025-11-01",
            endDate = "2025-11-03",
            type = "Personal"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var approveResponse = await client.PutAsync($"/api/v1/leave-requests/{leave!.Id}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Approve_ReturnsOk_ForManagerInSameDepartment()
    {
        using var hrClient = CreateAuthenticatedClient("hr");
        var departmentId = await TenantIsolationFixture.CreateDepartmentAsync(hrClient);
        var employeeId = await TenantIsolationFixture.CreateEmployeeInDepartmentAsync(hrClient, departmentId);

        var employeeUserId = Guid.NewGuid();
        var employeeRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.EmployeeSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient, employeeUserId, employeeRoleId, employeeId);

        using var employeeClient = CreateAuthenticatedClient("guest", employeeUserId);
        var createResponse = await employeeClient.PostAsJsonAsync("/api/v1/leave-requests", new
        {
            employeeId,
            startDate = "2025-12-01",
            endDate = "2025-12-05",
            type = "Annual"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var leave = await createResponse.Content.ReadFromJsonAsync<LeaveRequestResponse>(JsonOptions);

        var managerUserId = Guid.NewGuid();
        var managerRoleId = await TenantIsolationFixture.GetRoleIdBySlugAsync(
            hrClient, SystemRoleTemplates.ManagerSlug);
        await TenantIsolationFixture.CreateMembershipAsync(
            hrClient,
            managerUserId,
            managerRoleId,
            attributes: new Dictionary<string, string> { ["departmentId"] = departmentId.ToString() });

        using var managerClient = CreateAuthenticatedClient("guest", managerUserId);
        var approveResponse = await managerClient.PutAsync($"/api/v1/leave-requests/{leave!.Id}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<Guid> GetEmployeeIdAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/me");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.Content.ReadFromJsonAsync<MeDto>(JsonOptions);
        me!.EmployeeId.Should().NotBeNull();
        return me.EmployeeId!.Value;
    }

    private sealed record LeaveRequestResponse(Guid Id);
}
